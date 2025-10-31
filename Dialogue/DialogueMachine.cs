using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Nickel;
using static TheJazMaster.UnseenEffort.Translate;

namespace TheJazMaster.UnseenEffort;

/**
ver.0.18

To get DialogueMachine and the custom dialogue stuff working:
- edit the namespace of this file to at least match your project namespace
- Instantiate LocalDB in ModEntry.cs *after* all the dialogue has been added (or in a helper.Events.OnModLoadFinished AfterDbInit presented as such below):
        helper.Events.OnModLoadPhaseFinished += (_, phase) =>
        {
            if (phase == ModLoadPhase.AfterDbInit)
            {
                localDB = new(helper, package);
            }
        };
- Then register the locale of your dialogue by calling the instantiated LocalDB's GetLocalizationResults() in helper.Events.OnLoadStringsForLocale:
        helper.Events.OnLoadStringsForLocale += (_, thing) =>
        {
            foreach (KeyValuePair<string, string> entry in localDB.GetLocalizationResults())
            {
                thing.Localizations[entry.Key] = entry.Value;
            }
        };
- You're all set!
(when you're adding dialogue, you should use LocalDB.DumpStoryToLocalLocale())
*/

public enum DMod
{
    dialogue,
    switchsay,
    retain,
    instruction,
    title,
}
public enum EMod
{
    countFromStart,
    countFromEnd,
    findSwitchWithHash
}


public class AbstractThing
{
    public string? who;
    public string? loopTag;
    public string? what;
    public bool flipped;
    public bool ifCrew;
    public double delay;
    public string? choiceFunc;
}


public class EditThing : AbstractThing
{
    public int? switchNumber;
    public EMod searchConfig;
    public string? hashSearch;
    public EditThing(EMod searchConfig, int switchNumber, string who, string loopTag, string what, bool flipped = false, bool ifCrew = false, double delay = 0.0, string? choiceFunc = null)
    {
        this.searchConfig = searchConfig;
        this.who = who;
        this.loopTag = loopTag;
        this.what = what;
        this.flipped = flipped;
        this.ifCrew = ifCrew;
        this.delay = delay;
        this.choiceFunc = choiceFunc;
        this.switchNumber = switchNumber;
    }
    public EditThing(EMod searchConfig, int switchNumber, string who, string what, bool flipped = false, bool ifCrew = false, double delay = 0.0, string? choiceFunc = null)
    {
        this.searchConfig = searchConfig;
        this.who = who;
        this.what = what;
        this.flipped = flipped;
        this.ifCrew = ifCrew;
        this.delay = delay;
        this.choiceFunc = choiceFunc;
        this.switchNumber = switchNumber;
    }
    public EditThing(string hashToFind, string who, string loopTag, string what, bool flipped = false, bool ifCrew = false, double delay = 0.0, string? choiceFunc = null)
    {
        this.searchConfig = EMod.findSwitchWithHash;
        this.who = who;
        this.loopTag = loopTag;
        this.what = what;
        this.flipped = flipped;
        this.ifCrew = ifCrew;
        this.delay = delay;
        this.choiceFunc = choiceFunc;
        this.hashSearch = hashToFind;
    }
    public EditThing(string hashToFind, string who, string what, bool flipped = false, bool ifCrew = false, double delay = 0.0, string? choiceFunc = null)
    {
        this.searchConfig = EMod.findSwitchWithHash;
        this.who = who;
        this.what = what;
        this.flipped = flipped;
        this.ifCrew = ifCrew;
        this.delay = delay;
        this.choiceFunc = choiceFunc;
        this.hashSearch = hashToFind;
    }
}

public class DialogueThing : AbstractThing
{
    public string? title;
    public List<DialogueThing>? saySwitch;
    public DMod mode;
    public Instruction? instruction;

    /// <summary>
    /// A dialogue with emotions and all
    /// </summary>
    /// <param name="who">Who speaketh?</param>
    /// <param name="loopTag">How emote?</param>
    /// <param name="what">What they sayeth?</param>
    public DialogueThing(string who, string loopTag, string what, bool flipped = false, bool ifCrew = false, double delay = 0.0, string? choiceFunc = null)
    {
        this.mode = DMod.dialogue;
        this.who = who;
        this.loopTag = loopTag;
        this.what = what;
        this.flipped = flipped;
        this.ifCrew = ifCrew;
        this.delay = delay;
        this.choiceFunc = choiceFunc;
    }
    /// <summary>
    /// A dialogue with neutral emotion
    /// </summary>
    /// <param name="who">Who speaketh?</param>
    /// <param name="what">What they sayeth?</param>
    public DialogueThing(string who, string what, bool flipped = false, bool ifCrew = false, double delay = 0.0, string? choiceFunc = null)
    {
        this.mode = DMod.dialogue;
        this.who = who;
        this.what = what;
        this.flipped = flipped;
        this.ifCrew = ifCrew;
        this.delay = delay;
        this.choiceFunc = choiceFunc;
    }
    /// <summary>
    /// Adds a spacer that will allow the original text to fill in if the mod order is suboptimal. ONLY USED FOR ADDING TO EXISTING DIALOGUE.
    /// </summary>
    public DialogueThing()
    {
        this.mode = DMod.retain;
    }
    /// <summary>
    /// For adding any instructions unfulfilled by this dialogue thing
    /// </summary>
    /// <param name="instruction">Instructions to add</param>
    public DialogueThing(Instruction instruction)
    {
        this.mode = DMod.instruction;
        this.instruction = instruction;
    }
    /// <summary>
    /// For adding text to title cards
    /// </summary>
    /// <param name="title">The title to show (NULL for empty=true)</param>
    public DialogueThing(string? title)
    {
        this.mode = DMod.title;
        this.title = title;
    }

    /// <summary>
    /// Practically a SaySwitch. The list cannot contain anything but just dialogue.
    /// </summary>
    /// <param name="saySwitch">A list of Dialogue to go in 'ere</param>
    public DialogueThing(List<DialogueThing> saySwitch)
    {
        this.mode = DMod.switchsay;
        this.saySwitch = saySwitch;
    }

    /// <summary>
    /// For advanced stuff
    /// </summary>
    /// <param name="mode">Mode of dialogue</param>
    /// <param name="who">Whomst'd've</param>
    /// <param name="loopTag">Emotion</param>
    /// <param name="what">What they say?</param>
    /// <param name="flipped">Flipped to other side</param>
    /// <param name="ifCrew">???</param>
    /// <param name="delay">Delay</param>
    /// <param name="choiceFunc">Route choose</param>
    /// <param name="saySwitch">SaySwitch list</param>
    /// <param name="instruction">Custom instruction</param>
    /// <param name="title">Title</param>
    public DialogueThing(DMod mode, string? who = null, string? loopTag = null, string? what = null, bool flipped = false, bool ifCrew = false, double delay = 0.0, string? choiceFunc = null, List<DialogueThing>? saySwitch = null, Instruction? instruction = null, string? title = null)
    {
        this.mode = mode;
        this.who = who;
        this.loopTag = loopTag;
        this.what = what;
        this.flipped = flipped;
        this.ifCrew = ifCrew;
        this.delay = delay;
        this.choiceFunc = choiceFunc;
        this.saySwitch = saySwitch;
        this.instruction = instruction;
        this.title = title;
    }
}

public class DialogueMachine : StoryNode
{
    // public List<(string whoOrCommand, string? loopTag, string? what)> dialogue = null!;

    /// <summary>
    /// Edits existing dialogue by finding the switch you want to insert your dialogue into. Best used for vanilla dialogue. WILL IGNORE 'dialogue' DICTIONARY IF 'edit' IS USED
    /// </summary>
    public List<EditThing>? edit;
    /// <summary>
    /// Where all your dialogue *should* go. It can also support titles, mod dialogue edits, and custom instructions!
    /// </summary>
    public List<DialogueThing>? dialogue;

    /// <summary>
    /// Add the type of the artifact rather than trying to use the string key. Gets converted to hasArtifacts later.
    /// </summary>
    public List<Type>? hasArtifactTypes;
    /// <summary>
    /// Add the type of the artifact rather than trying to use the string key. Gets converted to doesNotHaveArtifacts later.
    /// </summary>
    public List<Type>? doesNotHaveArtifactTypes;
    /// <summary>
    /// Though any fields you declare will replace existing fields if you're modifying the original, lists and hashsets will be appended by default. Add the name of the list/hashset field if you want to completely replace them.
    /// </summary>
    public List<string>? replaceFields;

    /// <summary>
    /// Translates DialogueMachine into Instructions readable by LocalDB
    /// </summary>
    public void Convert(SimpleMod inst)
    {
        if (hasArtifactTypes is not null)
        {
            hasArtifacts ??= [];
            foreach (Type type in hasArtifactTypes)
            {
                // Modded
                if(inst.Helper?.Content?.Artifacts?.LookupByArtifactType(type) is IArtifactEntry iae) hasArtifacts.Add(iae.UniqueName);
                else if(DB.artifacts.ContainsValue(type)) hasArtifacts.Add(DB.artifacts.First(x => x.Value == type).Key);
                else inst.Logger.LogWarning($"Error when moving {type.Name} from [hasArtifactTypes] to [hasArtifacts]! Perhaps the artifact isn't registered yet or misspelt?");
            }
        }
        if (doesNotHaveArtifactTypes is not null)
        {
            doesNotHaveArtifacts ??= [];
            foreach (Type type in doesNotHaveArtifactTypes)
            {
                // Modded
                if(inst.Helper?.Content?.Artifacts?.LookupByArtifactType(type) is IArtifactEntry iae) doesNotHaveArtifacts.Add(iae.UniqueName);
                else if(DB.artifacts.ContainsValue(type)) doesNotHaveArtifacts.Add(DB.artifacts.First(x => x.Value == type).Key);
                else inst.Logger.LogWarning($"Error when moving {type.Name} from [doesNotHaveArtifactTypes] to [doesNotHaveArtifacts]! Perhaps the artifact isn't registered yet or misspelt?");
            }
        }
        if (edit is not null)  // Skips dialogue conversion if edits are available
        {
            foreach (EditThing e in edit)
            {
                lines.Add(e.searchConfig switch
                {
                    EMod.countFromStart => new InsertDialogueInSwitch
                    {
                        say = ConvertDialogueToSay(e),
                        whichSwitch = e.switchNumber
                    },
                    EMod.countFromEnd => new InsertDialogueInSwitch
                    {
                        say = ConvertDialogueToSay(e),
                        whichSwitch = e.switchNumber,
                        fromEnd = true
                    },
                    EMod.findSwitchWithHash => new InsertDialogueInSwitch
                    {
                        say = ConvertDialogueToSay(e),
                        whichHash = e.hashSearch
                    },
                    _ => new InsertDialogueInSwitch()  // should never occur
                });
            }
            return;
        }
        foreach (DialogueThing d in dialogue??=[])
        {
            lines.Add(ConvertDialogueToLine(d));
        }
    }

    /// <summary>
    /// Converts either DialogueThing or EditThing into a Say in a format the LocalDB converter will understand
    /// </summary>
    /// <param name="at"></param>
    /// <returns></returns>
    private static Say ConvertDialogueToSay(AbstractThing at)
    {
        return new Say
        {
            who = TC(at.who ?? ""),
            loopTag = at.loopTag,
            hash = at.what ?? "",
            flipped = at.flipped,
            ifCrew = at.ifCrew,
            delay = at.delay,
            choiceFunc = at.choiceFunc
        };
    }

    /// <summary>
    /// Converts a DialogueThing to an instruction so it can go in All[].lines
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    private static Instruction ConvertDialogueToLine(DialogueThing dt)
    {
        if (dt.mode == DMod.retain)
        {
            return new RetainOrig();
        }
        if (dt.mode == DMod.instruction && dt.instruction is not null)
        {
            return dt.instruction;
        }
        if (dt.mode == DMod.title)
        {
            if (dt.title is null)
            {
                return new TitleCard { empty = true };
            }
            else
            {
                return new TitleCard { hash = dt.title };
            }
        }
        if (dt.mode == DMod.switchsay && dt.saySwitch is not null)
        {
            SaySwitch ss = new()
            {
                lines = new()
            };
            foreach (DialogueThing dial in dt.saySwitch)
            {
                if (dial.mode == DMod.dialogue) ss.lines.Add(ConvertDialogueToSay(dial));
            }
            return ss;
        }

        return ConvertDialogueToSay(dt);
    }

    /// <summary>
    /// Eventually find a better way of tackling this (UNUSED)
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool CharExists(string name, SimpleMod inst)
    {
        name = TC(name);

        if (inst.Helper.Content?.Decks?.LookupByUniqueName(name) is not null) return true;

        if (DB.currentLocale.strings.ContainsKey("char." + name)) return true;  // this probably doesn't even work

        // if (ModEntry.Instance.Helper.Content?.Decks?.LookupByUniqueName($"{ModEntry.Instance.UniqueName}::{name}") is not null)
        // {
        //     return true;
        // }
        return false;
    }

    /// <summary>
    /// Checks if inputted string is a valid Artifact
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool ArtifactExists(string name, SimpleMod inst)
    {
        // Modded artifacts
        if (inst.Helper.Content?.Artifacts?.LookupByUniqueName(name) is not null) return true;
        // Game artifacts
        if (DB.artifacts.ContainsKey(name)) return true;
        return false;
    }
}


/// <summary>
/// Puts a placeholder for the original dialogue you're editing to fill in in that very spot.
/// </summary>
public class RetainOrig : Instruction
{
    public override string ToString()
    {
        return "RetainPlease";
    }
}

/// <summary>
/// Inserts a Say in the switch you're looking for
/// </summary>
public class InsertDialogueInSwitch : Instruction
{
    public Say say = null!;
    public int? whichSwitch;
    public bool fromEnd;
    public string? whichHash;
}


/// <summary>
/// Where the custom dialogue is saved, and shoved into the game at load
/// </summary>
public class LocalDB
{
    /// <summary>
    /// Default custom dialogue
    /// </summary>
    public static Story LocalStory { get; set; } = new();
    /// <summary>
    /// Coded custom dialogue for different locales. Please use DumpStoryToLocalLocale() to add your dialogue safely instead!
    /// </summary>
    public static Dictionary<string, Story> LocalStoryLocale { get; private set; } = new();
    /// <summary>
    /// Coded custom dialogue specifically for modded support. Please use DumpStoryToLocalLocale() to add your dialogue safely instead
    /// </summary>
    public static Dictionary<string, Dictionary<string, Story>> ModdedStoryLocale { get; private set; } = new();
    private int incrementingHash = 1;
    /// <summary>
    /// An incrementing hash. WARNING: Hash may conflict if under the same namespace!
    /// </summary>
    public int IncrementingHash
    {
        get => incrementingHash++;
    }
    /// <summary>
    /// The localisation dictionary with the generated hashes and dialogue, which gets to be added to the game's locale
    /// </summary>
    private readonly Dictionary<string, string> customLocalisation;

    /// <summary>
    /// Change ModEntry.Instance if necessary
    /// </summary>
    private static SimpleMod Inst => ModEntry.Instance;

    /// <summary>
    /// Should be instantiated *after* all the dialogues have been registered OR at Events.OnModLoadPhaseFinished, AfterDbInit.
    /// </summary>
    /// <param name="package"></param>
    public LocalDB(IModHelper helper, IPluginPackage<IModManifest> package)
    {
        customLocalisation = new();
        Story toUseStory;
        if (LocalStoryLocale.ContainsKey(DB.currentLocale.locale))  // For other coded translated dialogues
        {
            toUseStory = LocalStoryLocale[DB.currentLocale.locale];
            foreach (KeyValuePair<string, Dictionary<string, Story>> thing in ModdedStoryLocale)
            {
                if(helper.ModRegistry.LoadedMods.ContainsKey(thing.Key) && ModdedStoryLocale[thing.Key].TryGetValue(DB.currentLocale.locale, out Story? value))
                {
                    foreach (KeyValuePair<string, StoryNode> thing2 in value.all)
                    {
                        if (!toUseStory.all.TryAdd(thing2.Key, thing2.Value))
                        {
                            Inst.Logger.LogWarning("Could not add dialogue: " + thing2.Key);
                        }
                    }
                }
            }
        }
        else if (File.Exists($"{package.PackageRoot}\\i18n\\{DB.currentLocale.locale}_story.json"))  // For i18n translated story dialogue
        {
            toUseStory = Mutil.LoadJsonFile<Story>(package.PackageRoot.GetRelativeFile($"i18n/{DB.currentLocale.locale}_story.json").FullName);
        }
        else  // For default
        {
            toUseStory = LocalStory;
        }
        
        PasteToDB(toUseStory, DB.story);
    }

    /// <summary>
    /// This one must be used in Events.OnLoadStringsForLocale.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, string> GetLocalizationResults()
    {
        return customLocalisation;
    }

    /// <summary>
    /// A modded version that adds separate dictionaries for each mod key so they could be included/excluded on load
    /// </summary>
    /// <param name="locale"></param>
    /// <param name="modKey"></param>
    /// <param name="storyStuff"></param>
    public static void DumpStoryToLocalLocale(string locale, string modKey, Dictionary<string, DialogueMachine> storyStuff)
    {
        // Just to trigger the modded part on load lol
        if (!LocalStoryLocale.ContainsKey(locale))
        {
            LocalStoryLocale[locale] = new Story();
        }


        if (!ModdedStoryLocale.ContainsKey(modKey))
        {
            ModdedStoryLocale[modKey] = new();
        }
        if (!ModdedStoryLocale[modKey].ContainsKey(locale))
        {
            ModdedStoryLocale[modKey][locale] = new Story();
        }

        foreach (KeyValuePair<string, DialogueMachine> dm in storyStuff)
        {
            //ExistenceChecker(dm);

            // Tries to add the dialogue in the local locale local locale thing
            if (!ModdedStoryLocale[modKey][locale].all.TryAdd(dm.Key, dm.Value))
            {
                Inst.Logger.LogWarning("Could not add dialogue: " + dm.Key);
            }
        }
    }


    /// <summary>
    /// Adds a list of DialogueMachines to the appropriate locale, creating a new if locale doesn't exist.
    /// </summary>
    /// <param name="locale">the locale the storynodes will go in</param>
    /// <param name="storyStuff">Storynodes along with their keys to add to locale dictionary</param>
    public static void DumpStoryToLocalLocale(string locale, Dictionary<string, DialogueMachine> storyStuff)
    {
        if (!LocalStoryLocale.ContainsKey(locale))
        {
            LocalStoryLocale[locale] = new Story();
        }

        TC(storyStuff);

        foreach (KeyValuePair<string, DialogueMachine> dm in storyStuff)
        {
            // Tries to add the dialogue in the local locale local locale thing
            if (!LocalStoryLocale[locale].all.TryAdd(dm.Key, dm.Value))
            {
                Inst.Logger.LogWarning("Could not add dialogue: " + dm.Key);
            }
        }
    }

    private static void ExistenceChecker(KeyValuePair<string, StoryNode> sn)
    {
        #if DEBUG
        // Checks if the inputted artifact is a valid one that the game can check
        if (sn.Value.hasArtifacts is not null)
        {
            foreach (string artifact in sn.Value.hasArtifacts)
            {
                if (!DialogueMachine.ArtifactExists(artifact, Inst))
                {
                    Inst.Logger.LogWarning(sn.Key + "'s <hasArtifacts> may contain an erroneous artifact [" + artifact + "] that may not be recognized by the game! (or if it's a modded artifact: the mod isn't loaded.)");
                }
            }
        }
        if (sn.Value.doesNotHaveArtifacts is not null)
        {
            foreach (string artifact in sn.Value.doesNotHaveArtifacts)
            {
                if (!DialogueMachine.ArtifactExists(artifact, Inst))
                {
                    Inst.Logger.LogWarning(sn.Key + "'s <doesNotHaveArtifacts> may contain an erroneous artifact [" + artifact + "] that may not be recognized by the game! (or if it's a modded artifact: the mod isn't loaded.)");
                }
            }
        }

        // Checks if the inputted character is a valid one
        if (sn.Value.allPresent is not null)
        {
            foreach (string characer in sn.Value.allPresent)
            {
                if (!DialogueMachine.CharExists(characer, Inst))
                {
                    Inst.Logger.LogWarning(sn.Key + "'s <allPresent> may contain an erroneous character [" + characer + "] that may not be recognized by the game! (or if it's a modded character: the mod isn't loaded or is a modded enemy.)");
                }
            }
        }
        if (sn.Value.nonePresent is not null)
        {
            foreach (string characer in sn.Value.nonePresent)
            {
                if (!DialogueMachine.CharExists(characer, Inst))
                {
                    Inst.Logger.LogWarning(sn.Key + "'s <nonePresent> may contain an erroneous character [" + characer + "] that may not be recognized by the game! (or if it's a modded character: the mod isn't loaded or is a modded enemy.)");
                }
            }
        }

        if (sn.Value is DialogueMachine dm)
        {
            // Checks whether the who part in the edit or dialogues is not typo-d
            if (dm.edit is not null)
            {
                foreach (EditThing et in dm.edit)
                {
                    if (et.who is not null && !DialogueMachine.CharExists(et.who, Inst))
                    {
                        Inst.Logger.LogWarning(sn.Key + "'s <edit> contains a line with an invalid character [" + et.who + "] that may not be recognized by the game! Did you spell the character correctly? (or if it's a modded character: the mod isn't loaded or is a modded enemy.)");
                    }
                }
            }
            else if (dm.dialogue is not null)
            {
                foreach (DialogueThing dt in dm.dialogue)
                {
                    if (dt.saySwitch is not null)
                    {
                        foreach (DialogueThing dtss in dt.saySwitch)
                        {
                            if (dtss.who is not null && !DialogueMachine.CharExists(dtss.who, Inst))
                            {
                                Inst.Logger.LogWarning(sn.Key + "'s <dialogue(sayswitch)> contains a line with an invalid character [" + dtss.who + "] that may not be recognized by the game! Did you spell the character correctly? (or if it's a modded character: the mod isn't loaded or is a modded enemy.)");
                            }
                        }
                    }
                    else if (dt.who is not null && !DialogueMachine.CharExists(dt.who, Inst))
                    {
                        Inst.Logger.LogWarning(sn.Key + "'s <dialogue> contains a line with an invalid character [" + dt.who + "] that may not be recognized by the game! Did you spell the character correctly? (or if it's a modded character: the mod isn't loaded or is a modded enemy.)");
                    }
                }
            }

            // Checks if the edit dialogue thing's key is valid
            if (dm.edit is not null && !DB.story.all.ContainsKey(sn.Key))
            {
                Inst.Logger.LogWarning(sn.Key + " is trying to add to a dialogue that doesn't exist in game (yet)! If you're trying to edit modded dialogue, this may not be the appropriate way!");
            }
        }   
        #endif
    }

    /// <summary>
    /// Copies the storynodes from from to to, while also converting it for the game to recognise and registering the locales.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    private void PasteToDB(Story from, Story to)
    {
        foreach (KeyValuePair<string, StoryNode> sn in from.all)
        {
            bool editMode = false;
            // Convert all custom DialogueThings from DialogueMachine to StoryNode lines
            if (sn.Value is DialogueMachine dm)
            {
                ExistenceChecker(sn);
                dm.Convert(Inst);
                editMode = dm.edit is not null;
            }

            if (editMode)
            {
                to.all[sn.Key] = InjectALineIn(sn.Value, to.all[sn.Key], sn.Key);
                continue;
            }


            // Copy storynodes from from to to
            if (to.all.ContainsKey(sn.Key))
            {
                to.all[sn.Key] = StitchNodesTogether(sn.Value, to.all[sn.Key], sn.Key);
            }
            else
            {
                for (int a = 0; a < sn.Value.lines.Count; a++)
                {
                    MakeLinesRecognisable(sn.Value.lines[a], sn.Key);
                }
                to.all.Add(sn.Key, sn.Value);
            }
        }
    }


    /// <summary>
    /// Overrides the original field if source is different, and appends list fields (unless specified)
    /// </summary>
    /// <param name="target"></param>
    /// <param name="source"></param>
    private void CombineFields(ref StoryNode target, StoryNode source)
    {
        if(target is null || source is null) return;
        StoryNode defaultSource = new();
        List<Type> additionList = [typeof(List<string>), typeof(HashSet<string>), typeof(HashSet<Status>)];
        foreach (var field in typeof(StoryNode).GetFields(System.Reflection.BindingFlags.Public| System.Reflection.BindingFlags.Instance))
        {
            if (field.Name == "lines") continue;

            var sourceValue = field.GetValue(source);
            var defaultValue = field.GetValue(defaultSource);

            if(sourceValue is not null && !EqualityComparer<object>.Default.Equals(defaultValue, sourceValue))
            {
                if(!additionList.Contains(field.FieldType) || (source is DialogueMachine dm && dm.replaceFields is not null && dm.replaceFields.Contains(field.Name)))
                {
                    field.SetValue(target, sourceValue);
                }
                else
                {
                    var targetValue = field.GetValue(target);
                    if(sourceValue is List<string> l2)
                    {
                        List<string> l1 = (targetValue as List<string>)??[];
                        field.SetValue(target, l1.Concat(l2).ToList());
                    }
                    else if(sourceValue is HashSet<string> h2)
                    {
                        HashSet<string> h1 = (targetValue as HashSet<string>)??[];
                        field.SetValue(target, h1.Concat(h2).ToHashSet());
                    }
                    else if(sourceValue is HashSet<Status> h4)
                    {
                        HashSet<Status> h3 = (targetValue as HashSet<Status>)??[];
                        field.SetValue(target, h3.Concat(h4).ToHashSet());
                    }
                }
            }
        }
    }

    /// <summary>
    /// Safely inject a dialogue in an existing dialogue switch
    /// </summary>
    /// <param name="newStory"></param>
    /// <param name="existingStory"></param>
    /// <param name="script"></param>
    /// <returns>StoryNode with the injected dialogue</returns>
    private StoryNode InjectALineIn(in StoryNode newStory, in StoryNode existingStory, string script)
    {
        try
        {
            StoryNode result = existingStory;
            if (result.lines is not null)
            {
                foreach (Instruction instruction in newStory.lines)
                {
                    if (instruction is InsertDialogueInSwitch idis)
                    {
                        for (int a = 0, b = 0, c = result.lines.Count - 1; b < result.lines.Count && c >= 0; b++, c--)
                        {
                            if (!idis.fromEnd && result.lines[b] is SaySwitch ss)
                            {
                                if (idis.whichHash is not null)
                                {
                                    foreach (Say say in ss.lines)
                                    {
                                        if (say.hash == idis.whichHash)
                                        {
                                            ss.lines.Add(GetSayFromIDIS(idis, script));
                                            goto endofloop;
                                        }
                                    }
                                }
                                else if (idis.whichSwitch is not null && a == idis.whichSwitch)
                                {
                                    ss.lines.Add(GetSayFromIDIS(idis, script));
                                    goto endofloop;
                                }
                                a++;
                            }
                            else if (idis.fromEnd && result.lines[c] is SaySwitch bs)
                            {
                                if (idis.whichSwitch is not null && a == idis.whichSwitch)
                                {
                                    bs.lines.Add(GetSayFromIDIS(idis, script));
                                    goto endofloop;
                                }
                                a++;
                            }
                        }
                        Inst.Logger.LogWarning(script + "'s IDIS failed to find a switch to insert the dialogue into!");
                    }
                    endofloop:;
                }
            }
            CombineFields(ref result, newStory);
            return result;
        }
        catch (Exception err)
        {
            Inst.Logger.LogError(err, "Failed to edit a line with key:" + script);
            return existingStory;
        }
    }


    /// <summary>
    /// Grabs the Say from IDIS, then converts the Say to be game recognisable as well as registering the locale.
    /// </summary>
    /// <param name="idis"></param>
    /// <param name="script"></param>
    /// <returns></returns>
    private Say GetSayFromIDIS(InsertDialogueInSwitch idis, string script)
    {
        string what = idis.say.hash;
        idis.say.hash = $"{GetType().FullName}:{IncrementingHash}";
        customLocalisation[$"{script}:{idis.say.hash}"] = what;
        return idis.say;
    }


    /// <summary>
    /// Combines two storynodes together, automatically determining which of the two are original text based on the existence of RetainOrig
    /// </summary>
    /// <param name="newStory"></param>
    /// <param name="existingStory"></param>
    /// <param name="script"></param>
    /// <returns></returns>
    private StoryNode StitchNodesTogether(in StoryNode newStory, in StoryNode existingStory, string script)
    {
        try
        {
            StoryNode result = existingStory;  // Assumes that the existing story lines has already been converted from DialogueMachine recognized say to game recognized say
            if (existingStory.lines is not null)
            {
                StoryNode start = newStory;

                for (int x = 0; x < result.lines.Count && x < start.lines.Count; x++)
                {
                    if (result.lines[x].ToString() == "RetainPlease")
                    {
                        MakeLinesRecognisable(start.lines[x], script);
                        result.lines[x] = start.lines[x];
                    }
                    else if (result.lines[x] is Say or SaySwitch && start.lines[x] is Say or SaySwitch)
                    {
                        result.lines[x] = CombineTwoSays(result.lines[x], start.lines[x], script);
                    }
                }

                if (start.lines.Count > result.lines.Count)
                {
                    for (int y = result.lines.Count; y < start.lines.Count; y++)
                    {
                        MakeLinesRecognisable(start.lines[y], script);
                        result.lines.Add(start.lines[y]);
                    }
                }
                CombineFields(ref result, start);
            }
            return result;
        }
        catch (Exception err)
        {
            Inst.Logger.LogError(err, "Failed to edit a line with key:" + script);
            return existingStory;
        }
    }

    /// <summary>
    /// Combines two says, resulting in a SaySwitch
    /// </summary>
    /// <param name="existingLine"></param>
    /// <param name="newLine"></param>
    /// <param name="script"></param>
    /// <returns></returns>
    private SaySwitch CombineTwoSays(Instruction existingLine, Instruction newLine, string script)
    {
        SaySwitch result = new SaySwitch
        {
            lines = new()
        };
        if (existingLine is Say sayA)
        {
            result.lines.Add(sayA);
        }
        else if (existingLine is SaySwitch saySA)
        {
            result = saySA;
        }

        if (newLine is Say sayB)
        {
            MakeLinesRecognisable(sayB, script);
            result.lines.Add(sayB);
        }
        else if (newLine is SaySwitch saySB)
        {
            MakeLinesRecognisable(saySB, script);
            foreach (Say s in saySB.lines)
            {
                result.lines.Add(s);
            }
        }
        return result;
    }

    /// <summary>
    /// Converts a Say, SaySwitch, or Title's hash into a generated one, while adding to the localisation.
    /// </summary>
    /// <param name="instruction"></param>
    /// <param name="script"></param>
    public void MakeLinesRecognisable(Instruction instruction, string script)
    {
        if (instruction is Say say)
        {
            string what = say.hash;
            say.hash = $"{GetType().FullName}:{IncrementingHash}";
            //say.who = TryDeckLookup(say.who);
            customLocalisation[$"{script}:{say.hash}"] = what;
        }
        else if (instruction is SaySwitch saySwitch)
        {
            for (int a = 0; a < saySwitch.lines.Count; a++)
            {
                MakeLinesRecognisable(saySwitch.lines[a], script);
            }
        }
        else if (instruction is TitleCard title)
        {
            string what = title.hash;
            title.hash = $"{GetType().FullName}:{IncrementingHash}";
            if (title.empty is not true)
            {
                customLocalisation[$"{script}:{title.hash}"] = what;
            }
        }
    }
}