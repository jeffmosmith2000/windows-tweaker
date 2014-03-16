﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using WindowsTweaker.Models;
using Microsoft.Win32;

namespace WindowsTweaker.AppTasks {

    internal static class Utils {
        public static Func<int?, bool> IntToBool = (int? val) => !(val == 0 || !val.HasValue);
        
        public static Func<int?, bool> ReversedIntToBool = (int? val) => val == 0;

        public static Func<bool?, int> BoolToInt = (bool? val) => val == true ? 1 : 0;

        public static Func<bool?, int> ReversedBoolToInt = (bool? val) => val == true ? 0 : 1;

        public static Func<string, bool> StringToBool = (string val) => val != null && val.Equals("1");

        public static bool ReversedStringToBool(string val) {
            if (val == null)
                return false;
            return !val.Equals("1");
        }

        public static Func<bool?, string> BoolToString = (bool? val) => val == true ? "1" : "0";

        public static Func<bool?, string> ReversedBoolToString = (bool? val) => val == true ? "0" : "1";

        public static bool IsEmptyDirectory(string path) {
            return !Directory.EnumerateFiles(path).Any();
        }

        // Regex link : http://social.msdn.microsoft.com/Forums/en-US/e3f2bb04-1b6a-4e06-9fcd-07e86ae4bd5f/regular-expression-to-validate-file-path?forum=regexp
        private static readonly Regex FilePathRegex = new Regex(
            @"(?:(?:(?:\b[a-z]:|\\\\[a-z0-9_.$]+\\[a-z0-9_.$]+)\\|
	              \\?[^\\/:*?""<>|\r\n]+\\?)               
	              (?:[^\\/:*?""<>|\r\n]+\\)*               
	              [^\\/:*?""<>|\r\n]*)",
            RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public static string ExtractFilePath(string fPath) {
            Match filePathMatch = FilePathRegex.Match(fPath);
            if (!filePathMatch.Success) return null;
            string val = filePathMatch.Value;
            return val;
        }

        public static bool Move(this RegistryKey sourceKey, RegistryKey destinationKey, string keyName) {
            string keyVal = (string) sourceKey.GetValue(keyName);
            if (keyVal == null) return false;
            sourceKey.DeleteValue(keyName);
            destinationKey.SetValue(keyName, keyVal);
            return true;
        }

        public static string GetUserSelectedFilePath(string filter = "Executable files|*.exe|Batch files|*.bat|Command files|*.com|Jar Files|*.jar|All files|*.*") {
            OpenFileDialog openFileDialog = new OpenFileDialog {
                Filter = filter,
                Multiselect = false
            };
            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        internal static string SentenceJoin(this List<string> lst) {
            if (lst == null || !lst.Any())
                return String.Empty;
            string txt = String.Empty;
            int lenLst = lst.Count;
            switch (lenLst) {
                case 1:
                    return lst[0];
                case 2:
                    return lst[0] + " and " + lst[1];
                default:
                    return String.Join(", ", lst.Take(lenLst - 1)) + " and " + lst[lenLst - 1];
            }
        }

        internal static bool HasValueInShellCommand(this RegistryKey regKey, string valName) {
            if (regKey == null)
                return false;
            return regKey.GetSubKeyNames().Select(subKeyName => regKey.OpenSubKey(subKeyName + @"\" + Constants.Cmd)).
                Any(subKey => subKey != null && (subKey.GetValue("") as string) == valName);
        }

        public static bool HasValue(this RegistryKey regKey, string filePath) {
            if (regKey == null) return false;
            string[] valueKeys = regKey.GetValueNames();
            return (from valueKey in valueKeys
                select (string) regKey.GetValue(valueKey)
                into value
                where
                    value != null
                select ExtractFilePath(value)).Any(containedFilePath => containedFilePath
                                                                        != null &&
                                                                        containedFilePath.Equals(filePath,
                                                                            StringComparison.InvariantCultureIgnoreCase));
        }
    }
}