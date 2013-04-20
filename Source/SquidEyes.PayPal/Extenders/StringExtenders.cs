#region Copyright, Author Details and Related Context
//<notice lastUpdateOn="4/19/2013">
//  <assembly>SquidEyes.PayPal</assembly>
//  <description>A simple PayPal order-placement library</description>
//  <copyright>
//    Copyright (C) 2013 Louis S. Berman

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see http://www.gnu.org/licenses/.
//  </copyright>
//  <author>
//    <fullName>Louis S. Berman</fullName>
//    <email>louis@squideyes.com</email>
//    <website>http://squideyes.com</website>
//  </author>
//</notice>
#endregion 

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using System.Text.RegularExpressions;

namespace SquidEyes.PayPal
{
    internal static partial class StringExtenders
    {
        private static Dictionary<int, Regex> regexes =
            new Dictionary<int, Regex>();

        public static bool IsEmail(this string value)
        {
            const string PATTERN =
                @"^(([A-Za-z0-9]+_+)|([A-Za-z0-9]+\-+)|([A-Za-z0-9]+\.+)|([A-Za-z0-9]+\++))*[A-Za-z0-9]+@((\w+\-+)|(\w+\.))*\w{1,63}\.[a-zA-Z]{2,6}$";

            if (string.IsNullOrWhiteSpace(value))
                return false;

            if ((value.Length < 8) && (value.Length > 64))
                return false;

            return value.IsMatch(PATTERN);
        }

        public static bool IsPhone(this string value,
            PhoneKind phoneKind, bool allowExtension = false)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            const string EXTENSION_SUFFIX =
                @"(\s+?[xX]\d{1,5})?";

            const string DOMESTIC_PREFIX =
                @"[2-9]\d{2}\-[2-9]\d{2}\-\d{4}";

            const string INTERNATIONAL_PREFIX =
                @"(\(?\+?[0-9]*\)?)?[0-9_\- \(\)]*";

            if (value.Trim().Length == 0)
                return false;

            var pattern = new StringBuilder();

            pattern.Append('^');

            switch (phoneKind)
            {
                case PhoneKind.Domestic:
                    pattern.Append(DOMESTIC_PREFIX);
                    break;
                case PhoneKind.International:
                    pattern.Append(INTERNATIONAL_PREFIX);
                    break;
                case PhoneKind.EitherFormat:
                    pattern.Append("((");
                    pattern.Append(DOMESTIC_PREFIX);
                    pattern.Append(")|(");
                    pattern.Append(INTERNATIONAL_PREFIX);
                    pattern.Append("))");
                    break;
                case PhoneKind.NoFormat:
                    return (IsTrimmed(value, false));
            }

            if (allowExtension)
                pattern.Append(EXTENSION_SUFFIX);

            pattern.Append('$');

            return IsMatch(value, pattern.ToString());
        }

        public static bool IsMatch(this string value, string pattern)
        {
            return value.IsMatch(pattern, RegexOptions.None);
        }

        public static bool IsMatch(this string value,
            string pattern, RegexOptions options)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (string.IsNullOrWhiteSpace(pattern))
                return false;

            int hashCode = pattern.GetHashCode();

            Regex regex;

            if (!regexes.TryGetValue(hashCode, out regex))
            {
                options |= RegexOptions.Compiled;

                regex = new Regex(pattern, options);

                regexes.Add(hashCode, regex);
            }

            return regex.IsMatch(value);
        }

        public static bool IsTrimmed(this string value, int maxLength)
        {
            return value.IsTrimmed(1, maxLength);
        }

        public static bool IsTrimmed(this string value, int minLength, int maxLength)
        {
            if (value == (string)null)
                return false;

            if (minLength < 1)
                return false;

            if (maxLength < minLength)
                return false;

            if (value.Length < minLength)
                return false;

            if (value.Length > maxLength)
                return false;

            return (!char.IsWhiteSpace(value[0]) &&
                (!char.IsWhiteSpace(value[value.Length - 1])));
        }

        public static bool IsTrimmed(this string value, bool emptyOK = false)
        {
            if (value == (string)null)
                return false;

            if (value == string.Empty)
                return emptyOK;

            return (!char.IsWhiteSpace(value[0]) &&
                (!char.IsWhiteSpace(value[value.Length - 1])));
        }

        public static string TrimJson(this string json)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(json));

            return Regex.Replace(json, "(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+", "$1");
        }

        public static bool IsCardNumber(this string number)
        {
            CardType? cardType;

            return (number.IsCardNumber(out cardType));
        }

        public static bool IsCardNumber(this string number, out CardType? cardType)
        {
            Contract.Requires(number != null);

            int[] DELTAS = new int[] { 0, 1, 2, 3, 4, -4, -3, -2, -1, 0 };

            int checksum = 0;
            char[] chars = number.ToCharArray();

            for (int i = chars.Length - 1; i > -1; i--)
            {
                int j = ((int)chars[i]) - 48;

                checksum += j;

                if (((i - chars.Length) % 2) == 0)
                    checksum += DELTAS[j];
            }

            cardType = null;

            if ((checksum % 10) != 0)
                return false;

            switch (number[0])
            {
                case '3':
                    cardType = CardType.Amex;
                    return true;
                case '4':
                    cardType = CardType.Visa;
                    return true;
                case '5':
                    cardType = CardType.Mastercard;
                    return true;
                case '6':
                    cardType = CardType.Discover;
                    return true;
                default:
                    return false;
            }
        }

        public static CardType GetCardType(this string number)
        {
            Contract.Requires(number != null);

            CardType? cardType;

            if (!number.IsCardNumber(out cardType))
                throw new ArgumentOutOfRangeException("number");

            return cardType.Value;
        }
    }
}
