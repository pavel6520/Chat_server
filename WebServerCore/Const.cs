using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore {
    public static class Const {
        public static readonly char[] SPLIT_EQUAL = new char[] { '=' };
        public static readonly char[] SPLIT_SPACE = new char[] { ' ' };
        public static readonly char[] SPLIT_SLASH = new char[] { '/' };
        public static readonly char[] SPLIT_DOT = new char[] { ',' };
        public static readonly string[] SPLIT_DOTSPACE = new string[] { ", " };
        public static readonly string[] SPLIT_SEMICOLON = new string[] { "; " };
    }
}
