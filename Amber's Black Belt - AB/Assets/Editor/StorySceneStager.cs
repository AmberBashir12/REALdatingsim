using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using ActionType = StoryScene.Sentence.Action.Type;
using SceneAction = StoryScene.Sentence.Action;

public static partial class StorySceneActionProcessor
{
    private static List<Speaker> candidateCache;

    private static List<Speaker> GetCandidates()
    {
        if (candidateCache == null)
        {
            candidateCache = AssetDatabase.FindAssets("t:Speaker", new[] { "Assets/1Story/Speakers" })
                .Select(guid => AssetDatabase.LoadAssetAtPath<Speaker>(AssetDatabase.GUIDToAssetPath(guid)))
                .Where(speaker => speaker != null && IsRenderable(speaker) && !IsPlayer(speaker))
                .ToList();
        }

        return candidateCache;
    }

    private static bool IsNarrator(Speaker speaker)
    {
        return speaker != null && speaker.speakerName == "Narrator";
    }

    private static bool IsPlayer(Speaker speaker)
    {
        return speaker != null && AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(speaker)) == PlayerGuid;
    }

    private static bool IsRenderable(Speaker speaker)
    {
        return speaker != null
            && !IsNarrator(speaker)
            && speaker.prefab != null
            && speaker.sprites != null
            && speaker.sprites.Any(sprite => sprite != null);
    }

    // Two characters sit at -200/200; larger casts spread evenly across -450..450.
    private static Dictionary<Speaker, Vector2> GetPositions(List<Speaker> cast)
    {
        Dictionary<Speaker, Vector2> positions = new Dictionary<Speaker, Vector2>();
        for (int i = 0; i < cast.Count; i++)
        {
            float x;
            if (cast.Count == 1)
            {
                x = 0f;
            }
            else if (cast.Count == 2)
            {
                x = i == 0 ? -200f : 200f;
            }
            else
            {
                x = Mathf.Lerp(-450f, 450f, i / (float)(cast.Count - 1));
            }

            positions[cast[i]] = new Vector2(x, 0f);
        }

        return positions;
    }

    // Decides when each character enters or leaves a scene from who speaks and from narrator cues.
    private sealed class SceneStager
    {
        private static readonly Regex ArrivalRegex = BuildCueRegex(
            "arrive", "appear", "steps out", "steps in", "steps up", "walks in", "walks up", "walks over",
            "comes in", "comes up", "comes over", "runs in", "runs up", "hurries over", "hurries in",
            "enters", "approaches", "joins", "spots", "shows up", "calls out", "eyes meet", "grabs",
            "notices", "introduces");

        private static readonly Regex LeaveRegex = BuildCueRegex(
            "leaves", "dashes off", "runs off", "storms off", "storms out", "walks away", "walks off",
            "walks out", "heads out", "exits", "disappears", "hurries off", "slips out", "runs away");

        private static readonly Regex SentenceSplit = new Regex(@"(?<=[.!?])\s+");
        private static readonly Regex SecondPerson = new Regex(@"\b(?:you|your)\b", RegexOptions.IgnoreCase);

        private readonly StoryScene scene;
        private readonly string assetPath;
        private readonly List<Speaker> candidates;
        private readonly HashSet<string> skippedSpeakers;
        private readonly Dictionary<Speaker, Vector2> positions;
        private readonly HashSet<Speaker> present = new HashSet<Speaker>();
        private readonly List<(Speaker speaker, Regex name)> names = new List<(Speaker speaker, Regex name)>();
        private readonly List<Speaker> anchors = new List<Speaker>();

        public readonly List<Speaker> Cast = new List<Speaker>();
        public int StagedLines;

        // A null positions map makes this a dry run that only discovers the cast and never writes to the scene.
        public SceneStager(StoryScene scene, string assetPath, List<Speaker> candidates, HashSet<string> skippedSpeakers, Dictionary<Speaker, Vector2> positions)
        {
            this.scene = scene;
            this.assetPath = assetPath;
            this.candidates = candidates;
            this.skippedSpeakers = skippedSpeakers;
            this.positions = positions;
        }

        public void Run()
        {
            Prepare();
            present.Clear();
            Cast.Clear();
            StagedLines = 0;

            for (int i = 0; i < scene.sentences.Count; i++)
            {
                StoryScene.Sentence sentence = scene.sentences[i];
                if (sentence.sentenceType == StoryScene.Sentence.SentenceType.ENDING)
                {
                    continue;
                }

                bool isChoice = sentence.sentenceType == StoryScene.Sentence.SentenceType.CHOICE;
                List<SceneAction> staged = Stage(sentence.speaker, isChoice ? null : sentence.text, sentence.actions, i == 0, present);
                sentence.actions = Commit(staged, sentence.actions);
                StageSentenceOptions(sentence.choice.options);
                scene.sentences[i] = sentence;
                StagedLines++;
            }
        }

        private static Regex BuildCueRegex(params string[] cues)
        {
            return new Regex(@"\b(?:" + string.Join("|", cues.Select(Regex.Escape)) + ")", RegexOptions.IgnoreCase);
        }

        private static void AddUnique(List<Speaker> list, Speaker speaker)
        {
            if (speaker != null && !list.Contains(speaker))
            {
                list.Add(speaker);
            }
        }

        private List<SceneAction> Commit(List<SceneAction> staged, List<SceneAction> original)
        {
            return positions != null ? staged : original;
        }

        private Vector2 PositionOf(Speaker speaker)
        {
            return positions != null && positions.TryGetValue(speaker, out Vector2 position) ? position : Vector2.zero;
        }

        private void Prepare()
        {
            names.Clear();
            anchors.Clear();

            List<Speaker> speakers = new List<Speaker>();
            List<Speaker> actionSpeakers = new List<Speaker>();
            foreach (var line in AllLines())
            {
                AddUnique(speakers, line.speaker);
                foreach (SceneAction action in line.actions ?? new List<SceneAction>())
                {
                    AddUnique(actionSpeakers, action.speaker);
                }
            }

            foreach (Speaker speaker in speakers.Concat(actionSpeakers))
            {
                if (!IsNarrator(speaker) && !IsRenderable(speaker))
                {
                    skippedSpeakers.Add(speaker.speakerName);
                }
            }

            IEnumerable<IGrouping<string, Speaker>> byName = candidates
                .Where(candidate => !string.IsNullOrWhiteSpace(candidate.speakerName))
                .GroupBy(candidate => candidate.speakerName.Trim(), StringComparer.OrdinalIgnoreCase);
            foreach (IGrouping<string, Speaker> group in byName)
            {
                if (!Regex.IsMatch(group.Key, "[A-Za-z]{3}"))
                {
                    continue;
                }

                Speaker chosen = group.FirstOrDefault(candidate => speakers.Contains(candidate)) ?? group.First();
                string pattern = @"\b" + Regex.Escape(group.Key).Replace(@"\.", @"\.\s?") + @"(?:'?s)?\b";
                names.Add((chosen, new Regex(pattern, RegexOptions.IgnoreCase)));
            }

            HashSet<Speaker> triggered = new HashSet<Speaker>();
            foreach (var line in AllLines())
            {
                if (IsRenderable(line.speaker))
                {
                    triggered.Add(line.speaker);
                }

                if ((line.speaker == null || IsNarrator(line.speaker)) && line.text != null)
                {
                    ScanCues(line.text, out List<Speaker> arriving, out _);
                    triggered.UnionWith(arriving);
                }
            }

            // The player and anyone who is present without ever speaking or being cued are on screen from line one.
            Speaker player = speakers.Concat(actionSpeakers).FirstOrDefault(IsPlayer);
            if (player != null && IsRenderable(player))
            {
                anchors.Add(player);
            }

            foreach (Speaker speaker in actionSpeakers)
            {
                if (IsRenderable(speaker) && !IsPlayer(speaker) && !triggered.Contains(speaker) && !anchors.Contains(speaker))
                {
                    anchors.Add(speaker);
                }
            }
        }

        private List<SceneAction> Stage(Speaker speaker, string text, List<SceneAction> existing, bool sceneStart, HashSet<Speaker> state)
        {
            bool narration = speaker == null || IsNarrator(speaker);
            Speaker actor = IsRenderable(speaker) ? speaker : null;

            List<Speaker> arrivals = new List<Speaker>();
            if (sceneStart)
            {
                arrivals.AddRange(anchors.Where(anchor => !state.Contains(anchor)));
            }

            if (actor != null && !state.Contains(actor) && !arrivals.Contains(actor))
            {
                arrivals.Add(actor);
            }

            List<Speaker> leavers = new List<Speaker>();
            if (narration && text != null)
            {
                ScanCues(text, out List<Speaker> arriving, out List<Speaker> leaving);
                foreach (Speaker cued in arriving)
                {
                    if (!state.Contains(cued) && !arrivals.Contains(cued))
                    {
                        arrivals.Add(cued);
                    }
                }

                foreach (Speaker cued in leaving)
                {
                    if ((state.Contains(cued) || arrivals.Contains(cued)) && !leavers.Contains(cued))
                    {
                        leavers.Add(cued);
                    }
                }
            }

            List<SceneAction> authoredMoves = new List<SceneAction>();
            List<SceneAction> authoredLeaves = new List<SceneAction>();
            foreach (SceneAction authored in existing ?? new List<SceneAction>())
            {
                if (authored.speaker == null)
                {
                    continue;
                }

                if (authored.actionType == ActionType.MOVE)
                {
                    authoredMoves.Add(authored);
                }
                else if (authored.actionType == ActionType.DISAPPEAR && !leavers.Contains(authored.speaker))
                {
                    leavers.Add(authored.speaker);
                    authoredLeaves.Add(authored);
                }
            }

            foreach (Speaker arrival in arrivals)
            {
                if (!Cast.Contains(arrival))
                {
                    Cast.Add(arrival);
                }
            }

            List<SceneAction> actions = new List<SceneAction>();
            foreach (Speaker arrival in arrivals)
            {
                GetSpriteIndices(arrival, assetPath, out int neutralIndex, out _);
                actions.Add(CreateAction(arrival, neutralIndex, ActionType.APPEAR, PositionOf(arrival)));
            }

            foreach (SceneAction move in authoredMoves)
            {
                if (state.Contains(move.speaker) || arrivals.Contains(move.speaker))
                {
                    actions.Add(move);
                }
            }

            foreach (Speaker member in Cast.Where(cast => state.Contains(cast) || arrivals.Contains(cast)))
            {
                GetSpriteIndices(member, assetPath, out int neutralIndex, out int talkIndex);
                bool speaking = member == actor;
                actions.Add(CreateAction(
                    member,
                    speaking ? talkIndex : neutralIndex,
                    speaking ? ActionType.BOUNCE : ActionType.NONE,
                    PositionOf(member)));
            }

            foreach (Speaker leaver in leavers)
            {
                SceneAction authored = authoredLeaves.FirstOrDefault(leave => leave.speaker == leaver);
                actions.Add(authored.speaker != null ? authored : CreateAction(leaver, 0, ActionType.DISAPPEAR, PositionOf(leaver)));
            }

            state.UnionWith(arrivals);
            state.ExceptWith(leavers);
            return actions;
        }

        // After a choice only characters still on screen in every branch count as present.
        private void StageSentenceOptions(List<StoryScene.Sentence.ChoiceOption> options)
        {
            if (options == null)
            {
                return;
            }

            HashSet<Speaker> survivors = new HashSet<Speaker>(present);
            for (int i = 0; i < options.Count; i++)
            {
                StoryScene.Sentence.ChoiceOption option = options[i];
                HashSet<Speaker> branch = new HashSet<Speaker>(present);
                StageFollowUps(option.followUpSentences, branch);
                survivors.IntersectWith(branch);
                options[i] = option;
            }

            present.IntersectWith(survivors);
        }

        private void StageFollowUps(List<StoryScene.FollowUpSentence> followUps, HashSet<Speaker> state)
        {
            if (followUps == null)
            {
                return;
            }

            for (int i = 0; i < followUps.Count; i++)
            {
                StoryScene.FollowUpSentence followUp = followUps[i];
                if (followUp.followUpType == StoryScene.FollowUpSentence.FollowUpType.ENDING)
                {
                    continue;
                }

                bool isChoice = followUp.followUpType == StoryScene.FollowUpSentence.FollowUpType.CHOICE;
                List<SceneAction> staged = Stage(followUp.speaker, isChoice ? null : followUp.text, followUp.actions, false, state);
                followUp.actions = Commit(staged, followUp.actions);
                StageLineOptions(followUp.choice.options, state);
                followUps[i] = followUp;
                StagedLines++;
            }
        }

        private void StageLineOptions(List<StoryScene.FollowUpChoiceOption> options, HashSet<Speaker> state)
        {
            if (options == null)
            {
                return;
            }

            HashSet<Speaker> survivors = new HashSet<Speaker>(state);
            for (int i = 0; i < options.Count; i++)
            {
                StoryScene.FollowUpChoiceOption option = options[i];
                HashSet<Speaker> branch = new HashSet<Speaker>(state);
                if (option.followUpLines != null)
                {
                    for (int j = 0; j < option.followUpLines.Count; j++)
                    {
                        StoryScene.FollowUpLine line = option.followUpLines[j];
                        if (line.lineType == StoryScene.FollowUpLine.LineType.SENTENCE)
                        {
                            List<SceneAction> staged = Stage(line.speaker, line.text, line.actions, false, branch);
                            line.actions = Commit(staged, line.actions);
                            option.followUpLines[j] = line;
                            StagedLines++;
                        }
                    }
                }

                survivors.IntersectWith(branch);
                options[i] = option;
            }

            state.IntersectWith(survivors);
        }

        private void ScanCues(string text, out List<Speaker> arriving, out List<Speaker> leaving)
        {
            arriving = new List<Speaker>();
            leaving = new List<Speaker>();

            foreach (string sentenceText in SentenceSplit.Split(text))
            {
                Match leave = LeaveRegex.Match(sentenceText);
                if (leave.Success)
                {
                    Speaker leaver = NearestNameBefore(sentenceText, leave.Index);
                    if (leaver != null && !leaving.Contains(leaver))
                    {
                        leaving.Add(leaver);
                    }

                    continue;
                }

                if (!ArrivalRegex.IsMatch(sentenceText))
                {
                    continue;
                }

                IEnumerable<Speaker> mentioned = names
                    .Select(entry => (entry.speaker, match: entry.name.Match(sentenceText)))
                    .Where(entry => entry.match.Success)
                    .OrderBy(entry => entry.match.Index)
                    .Select(entry => entry.speaker);
                foreach (Speaker speaker in mentioned)
                {
                    if (!arriving.Contains(speaker))
                    {
                        arriving.Add(speaker);
                    }
                }
            }
        }

        // The closest name before the cue is the one leaving, unless the clause is about "you".
        private Speaker NearestNameBefore(string sentenceText, int cueIndex)
        {
            Speaker best = null;
            int bestEnd = -1;
            foreach (var entry in names)
            {
                foreach (Match match in entry.name.Matches(sentenceText))
                {
                    int end = match.Index + match.Length;
                    if (end <= cueIndex && end > bestEnd)
                    {
                        best = entry.speaker;
                        bestEnd = end;
                    }
                }
            }

            if (best == null)
            {
                return null;
            }

            string gap = sentenceText.Substring(bestEnd, cueIndex - bestEnd);
            return gap.Length <= 30 && !SecondPerson.IsMatch(gap) ? best : null;
        }

        private IEnumerable<(Speaker speaker, string text, List<SceneAction> actions)> AllLines()
        {
            foreach (StoryScene.Sentence sentence in scene.sentences)
            {
                if (sentence.sentenceType == StoryScene.Sentence.SentenceType.ENDING)
                {
                    continue;
                }

                bool isChoice = sentence.sentenceType == StoryScene.Sentence.SentenceType.CHOICE;
                yield return (sentence.speaker, isChoice ? null : sentence.text, sentence.actions);

                foreach (StoryScene.Sentence.ChoiceOption option in sentence.choice.options ?? new List<StoryScene.Sentence.ChoiceOption>())
                {
                    foreach (var line in FollowUpLines(option.followUpSentences))
                    {
                        yield return line;
                    }
                }
            }
        }

        private static IEnumerable<(Speaker speaker, string text, List<SceneAction> actions)> FollowUpLines(List<StoryScene.FollowUpSentence> followUps)
        {
            foreach (StoryScene.FollowUpSentence followUp in followUps ?? new List<StoryScene.FollowUpSentence>())
            {
                if (followUp.followUpType == StoryScene.FollowUpSentence.FollowUpType.ENDING)
                {
                    continue;
                }

                bool isChoice = followUp.followUpType == StoryScene.FollowUpSentence.FollowUpType.CHOICE;
                yield return (followUp.speaker, isChoice ? null : followUp.text, followUp.actions);

                foreach (StoryScene.FollowUpChoiceOption option in followUp.choice.options ?? new List<StoryScene.FollowUpChoiceOption>())
                {
                    foreach (StoryScene.FollowUpLine line in option.followUpLines ?? new List<StoryScene.FollowUpLine>())
                    {
                        if (line.lineType == StoryScene.FollowUpLine.LineType.SENTENCE)
                        {
                            yield return (line.speaker, line.text, line.actions);
                        }
                    }
                }
            }
        }
    }
}
