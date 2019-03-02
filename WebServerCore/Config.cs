using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;

namespace WebServerCore {
    public static class Config {
        private static string _file;
        private static Body body;
        public static MySQLstr mysql;
        public static HTTPstr http;
        public static HTTPSstr https;

        public static bool Read(string file) {
            bool result = false;
            _file = file;
            XmlSerializer formatter = new XmlSerializer(typeof(Body));
            try {
                FileStream fs = new FileStream(_file, FileMode.Open);
                body = (Body)formatter.Deserialize(fs);
                result = true;
                Log.Write(false, "INFO", "Config", $"Файл {file} успешно прочитан");
            }
            catch {
                try {
                    FileStream fs = new FileStream(_file, FileMode.Create);
                    body = new Body();
                    body.mysql = new MySQLstr { login = "root", pass = "pass", host = "localhost", port = 3306, db = "chat" };
                    body.http = new HTTPstr { port = 80 };
                    body.https = new HTTPSstr { port = 443 };
                    formatter.Serialize(fs, body);
                }
                catch (Exception ex) {
                    Log.Write(true, "FATAL", "Config", $"Не удалось создать файл {file} с ошбкой: {ex.Message}");
                    Environment.Exit(2);
                }
                Log.Write(true, "WARN", "Config", $"Не удалось открыть файл {file} либо он повржден. Был создан новый файл");
            }

            mysql = body.mysql;
            http = body.http;
            https = body.https;

            return result;
        }

        public class Body {
            public MySQLstr mysql;
            public HTTPstr http;
            public HTTPSstr https;
        }

        public struct MySQLstr {
            public string host;
            public int port;
            public string login;
            public string pass;
            public string db;
        }
        public struct HTTPstr {
            public int port;
        }
        public struct HTTPSstr {
            public int port;
        }

    }

    //class INI
    //{
    //    string Path; //Имя файла.

    //    [DllImport("kernel32")] // Подключаем kernel32.dll и описываем его функцию WritePrivateProfilesString
    //    static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

    //    [DllImport("kernel32")] // Еще раз подключаем kernel32.dll, а теперь описываем функцию GetPrivateProfileString
    //    static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

    //    // С помощью конструктора записываем пусть до файла и его имя.
    //    public INI(string IniPath)
    //    {
    //        Path = new FileInfo(IniPath).FullName.ToString();
    //    }

    //    //Читаем ini-файл и возвращаем значение указного ключа из заданной секции.
    //    public string ReadINI(string Section, string Key)
    //    {
    //        var RetVal = new StringBuilder(255);
    //        GetPrivateProfileString(Section, Key, "", RetVal, 255, Path);
    //        return RetVal.ToString();
    //    }
    //    //Записываем в ini-файл. Запись происходит в выбранную секцию в выбранный ключ.
    //    public void WriteINI(string Section, string Key, string Value)
    //    {
    //        WritePrivateProfileString(Section, Key, Value, Path);
    //    }

    //    //Удаляем ключ из выбранной секции.
    //    public void DeleteKey(string Section, string Key = null)
    //    {
    //        WriteINI(Section, Key, null);
    //    }
    //    //Удаляем выбранную секцию
    //    public void DeleteSection(string Section = null)
    //    {
    //        WriteINI(Section, null, null);
    //    }
    //    //Проверяем, есть ли такой ключ, в этой секции
    //    public bool KeyExists(string Section, string Key = null)
    //    {
    //        return ReadINI(Section, Key).Length > 0;
    //    }
    //}
}