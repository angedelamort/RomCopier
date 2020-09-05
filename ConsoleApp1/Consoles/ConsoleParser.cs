using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RomCopier.Consoles
{
    // TODO: maybe swap to a generic parser?
    // todo: find translations?
    public class ConsoleParser : IConsoleParser
    {
        private readonly List<string> countries = new List<string> { };

        private class Item
        {
            public Item(string filename)
            {
                var name = Path.GetFileName(filename);

                Filename = filename;

                var regexTitle = new Regex(@"^(.*?) \(");
                var match = regexTitle.Match(name);
                if (match.Success)
                    Title = match.Groups[1].Value;

                var regexParenthesis = new Regex(@"\(([^)]+?)\)");
                var matches = regexParenthesis.Matches(name);
                var i = 0;
                foreach (Match m in matches)
                {
                    var content = m.Groups[1].Value;
                    if (string.Compare(content, "beta", true) == 0 || string.Compare(content, "sample", true) == 0)
                        IsBeta = true;
                    else if (content.StartsWith("V", StringComparison.CurrentCultureIgnoreCase) && 
                        Version.TryParse(content.Substring(1), out var version))
                        Version = version;
                    else if (i == 0)
                    {
                        var splitResult = content.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var s in splitResult)
                            Countries.Add(s.ToLower());
                    }
                    else
                    {
                        Options.Add(content);
                    }

                    // second might be languages or other fields.
                    i++;
                }

                var regexFlags = new Regex(@"\[([^]]+)]");
                matches = regexFlags.Matches(name);
                foreach (Match m in matches)
                {
                    var flags = m.Groups[1].Value;
                    if (flags == "!")
                        Verified = true;
                    else if (flags.StartsWith("T")) // translation
                    {
                        // if stats with T => translation -> T+Por1.00_Bocafig
                        Flags.Add(flags);
                    }
                    else
                    {
                        if (flags.Contains('b'))
                            BadRom = true;
                    }
                }
            }

            // not sure it goes in here. Don't like the parameter
            public bool IsSupported(List<string> countries)
            {
                foreach (var c in Countries)
                {
                    if (countries.Contains(c))
                        return true;
                }
                
                return false;
            }

            /// <summary>
            /// Return true if the current item is better match than the other.
            /// </summary>
            /// <param name="item"></param>
            /// <returns></returns>
            public bool IsBetterMatchThan(Item item)
            {
                if (Verified && !item.Verified)
                    return true;

                if (!IsBeta && item.IsBeta)
                    return true;

                if (Version > item.Version)
                    return true;

                if (Flags.Count() < item.Flags.Count())
                    return true;

                if (Options.Count() < item.Options.Count())
                    return true;

                return false;
            }

            public List<string> Countries { get; } = new List<string>();
            public bool Verified { get; }
            public string Title { get; }
            public string Filename { get; }
            public bool IsBeta { get; }
            public Version Version { get; } = new Version("0.0.0");
            public bool BadRom { get; }
            public List<string> Flags { get; } = new List<string>();
            public List<string> Options { get; } = new List<string>();
        }
        /*
         Standard Codes ( ‡ explanations further down)
            [a] Alternate
            [p] Pirate
            [b] Bad Dump     (avoid these, they may not work!)
            [t] Trained
            [f] Fixed
            [T-] OldTranslation
            [T+] NewerTranslation
            [h] Hack
            (-) Unknown Year
            [o] Overdump
            [!] Verified Good Dump
            (M#) Multilanguage (# of Languages)
            (###) Checksum
            (??k) ROM Size
            ZZZ_ Unclassified
            (Unl) Unlicensed

          Game Boy
            [C] Color
            [S] Super
            [BF] Bung Fix 

          Super Nintendo
            (BS) BS ROMs
            (ST) Sufami Turbo
            (NP) Nintendo Power 

          Sega Genesis/Mega Drive
            (1) Japan
            (4) USA
            (5) NTSC Only
            (8) PAL Only
            [ (B) Brazil ]
            [ [c] Checksum ]
            [ [x] Bad Checksum]
            [ [R-] Countries ] 

          NES/Famicom
            [PC10] Playchoice 10 version
            [VS] Vs Version

          Country Codes
            (1) Japan & Korea
            (4) USA & Brazil - NTSC
            (A) Australia
            (J) Japan
            (B) Brazil
            (K) Korea
            (C) China
            (NL) Netherlands
            (E) Europe
            (PD) Public Domain
            (F) France
            (S) Spain
            (F) World (Genesis)
            (FC) French Canadian
            (SW) Sweden
            (FN) Finland
            (U) USA
            (G) Germany
            (UK) England
            (GR) Greece
            (Unk) Unknown Country
            (HK) Hong Kong
            (I) Italy
            (H) Holland
            (Unl) Unlicensed
         */

        public List<string> Filter(List<string> files)
        {
            var regex = new Regex(@"^([^(]+).*\.zip");
            var dictionary = new Dictionary<string, Item>();
            foreach (var file in files)
            {
                var item = new Item(file);
                if (item.BadRom || !item.IsSupported(countries))
                    continue;

                if (dictionary.TryGetValue(item.Title, out var existingItem))
                {
                    if (item.IsBetterMatchThan(existingItem))
                        dictionary[item.Title] = item;
                }
                else
                {
                    dictionary[item.Title] = item;
                }
            }

            return dictionary.Values.Select(x => x.Filename).ToList();
        }

        public void Init(string countriesString)
        {
            var countriesArray = countriesString.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

            // NOTE: everything should be lower case for fast comparison later.
            foreach (var countryString in countriesArray)
            {
                if (Enum.TryParse<Country>(countryString, true, out var countryType))
                {
                    switch (countryType)
                    {
                        case Country.USA:
                            countries.Add("u");
                            countries.Add("usa");
                            break;
                        case Country.Japan:
                            countries.Add("j");
                            countries.Add("japan");
                            break;
                        case Country.Europe:
                            countries.Add("e");
                            countries.Add("europe");
                            break;
                        case Country.World:
                            countries.Add("world");
                            break;
                        default:
                            // not supported for now....
                            break;
                    }
                }
            }
        }
    }
}
