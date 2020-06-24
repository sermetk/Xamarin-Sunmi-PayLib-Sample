using System;
using System.Collections.Generic;
using System.Linq;
using Android.OS;
using Android.Text;

namespace XamarinSunmiPayLibSample.Droid.PayImplementations
{
    public static class Helpers
    {
        public static string JavaSubString(this string s, int start, int end)
        {
            return s[start..end];
        }
        public static string BundletoString(this Bundle bundle)
        {
            if (bundle == null)
                return string.Empty;
            var sb = new Java.Lang.StringBuilder();
            var set = bundle.KeySet();
            foreach (var item in set)
            {
                sb.Append(item);
                sb.Append(":");
                sb.Append(bundle.Get(item));
                sb.Append("\n");
            }
            if (sb.Length() > 0)
                sb.DeleteCharAt(sb.Length() - 1);
            return sb.ToString();
        }
        public static string Bytes2HexStr(byte[] bytes)
        {
            return string.Join(string.Empty, bytes.Select(b => b.ToString("x2")).ToArray());
        }
        private static byte CharToByte(char c)
        {
            return (byte)"0123456789ABCDEF".IndexOf(c);
        }
        public static byte[] GetBytesFromHexString(string hexstring)
        {
            if (string.IsNullOrEmpty(hexstring))
                return null;
            hexstring = hexstring.Replace(" ", string.Empty);
            hexstring = hexstring.ToUpper();
            int size = hexstring.Length / 2;
            char[] hexarray = hexstring.ToCharArray();
            byte[] rv = new byte[size];
            for (int i = 0; i < size; i++)
            {
                int pos = i * 2;
                rv[i] = (byte)(CharToByte(hexarray[pos]) << 4 | CharToByte(hexarray[pos + 1]));
            }
            return rv;
        }
        public static Dictionary<string, (string tag, int length, string value)> BuildTLVMap(string hexStr)
        {
            var map = new Dictionary<string, (string tag, int length, string value)>();
            if (string.IsNullOrEmpty(hexStr) || hexStr.Length % 2 != 0)
                return map;
            var position = 0;
            while (position != hexStr.Length)
            {
                var tupleTag = GetTag(hexStr, position);
                if (TextUtils.IsEmpty(tupleTag.a) || "00".Equals(tupleTag.a))
                    break;
                var tupleLen = GetLength(hexStr, tupleTag.b);
                var tupleValue = GetValue(hexStr, tupleLen.b, tupleLen.a);
                map.Add(tupleTag.a, (tupleTag.a, tupleLen.a, tupleValue.a));
                position = tupleValue.b;
            }
            return map;
        }
        private static (string a, int b) GetTag(string hexString, int position)
        {
            var byte1 = hexString.JavaSubString(position, position + 2);
            var byte2 = hexString.JavaSubString(position + 2, position + 4);
            int b1 = Convert.ToInt32(byte1, 16);
            int b2 = Convert.ToInt32(byte2, 16);
            string tag;
            if ((b1 & 0x1F) == 0x1F)
            {
                if ((b2 & 0x80) == 0x80)
                    tag = hexString.JavaSubString(position, position + 6);
                else
                    tag = hexString.JavaSubString(position, position + 4);
            }
            else
            {
                tag = hexString.JavaSubString(position, position + 2);
            }
            return (tag.ToUpper(), position + tag.Length);
        }
        private static (int a, int b) GetLength(string hexString, int position)
        {
            var index = position;
            var hexLen = hexString.JavaSubString(index, index + 2);
            index += 2;
            var byte1 = Convert.ToInt32(hexLen, 16);
            if ((byte1 & 0x80) != 0)
            {
                var subLen = byte1 & 0x7F;
                hexLen = hexString.JavaSubString(index, index + subLen * 2);
                index += subLen * 2;
            }
            return (Convert.ToInt32(hexLen, 16), index);
        }
        private static (string a, int b) GetValue(string hexStr, int position, int len)
        {
            var value = hexStr.JavaSubString(position, position + len * 2);
            return (value.ToUpper(), position + len * 2);
        }
    }

}
