using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WireSplitter
{
    class Wire
    {
        public string a { get; set; } //file header
        public string b { get; set; } //group header
        public string c { get; set; } //group trailer
        public string d { get; set; } //file trailer
    }
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
                return;
            var f = Path.GetFileName(args[0]);
            Console.WriteLine("working file: " + f);
            using (StreamReader r = new StreamReader(f))
            {
                string l;
                string s = Directory.GetCurrentDirectory();
                int p = 0; //payment count
                Wire w = new Wire();
                while((l = r.ReadLine()) != null)
                {
                    if (l.Substring(0, 2) == "01") //file header
                    {
                        w.a = l + Environment.NewLine;
                    } else if (l.Substring(0, 2) == "02") //group header
                    {
                        w.b = l + Environment.NewLine;
                    } else if (l.Substring(0,2) == "98") //group trailer
                    {
                        w.c = l + Environment.NewLine;
                        for(int i = 1; i < p + 1; i++)
                        {
                            File.AppendAllText(s + @"\split_wire" + i + @".dat", w.c);
                        }
                    } else if(l.Substring(0,2) == "99") //file trailer
                    {
                        w.d = l + Environment.NewLine;
                        for (int i = 1; i < p + 1; i++)
                        {
                            File.AppendAllText(s + @"\split_wire" + i + @".dat", w.d);
                        }
                    } else if(l.Substring(0,2) == "03") //account identifier & summary status
                    {
                        p++;
                        l = l + Environment.NewLine;
                        File.AppendAllText(s + @"\split_wire" + p + @".dat", w.a);
                        File.AppendAllText(s + @"\split_wire" + p + @".dat", w.b);
                        File.AppendAllText(s + @"\split_wire" + p + @".dat", l);
                    } else if(l.Substring(0,2) == "16") //transaction detail
                    {

                        l = l + Environment.NewLine;
                        File.AppendAllText(s + @"\split_wire" + p + @".dat", l);
                    } else if(l.Substring(0,2) == "88") //continuation record
                    {
                        l = l + Environment.NewLine;
                        File.AppendAllText(s + @"\split_wire" + p + @".dat", l);
                    } else if(l.Substring(0,2) == "49")
                    {
                        l = l + Environment.NewLine;
                        File.AppendAllText(s + @"\split_wire" + p + @".dat", l);
                    }
                }
            }
        }
    }
}
