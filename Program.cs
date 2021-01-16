using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace WireSplitter
{
    class Wire
    {
        public string a { get; set; } //file header
        public string b { get; set; } //group header
        public string c { get; set; } //group trailer
        public string d { get; set; } //file trailer
        public decimal e { get; set; } //temp placement for decimal
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
                List<string> ts = new List<string>();
                Wire w = new Wire();
                while((l = r.ReadLine()) != null)
                {
                    if (l.Substring(0, 2) == "01") //file header
                    {
                        w.a = l + Environment.NewLine;
                    } else if (l.Substring(0, 2) == "02") //group header
                    {
                        w.b = l + Environment.NewLine;
                    } else if (l.Substring(0, 2) == "98") //group trailer
                    {
                        string[] h = { "02", "03", "16", "49", "88", "98" };
                        string[] g = { "03" };
                        var a = l.Substring((l.IndexOf(",") + 1));
                        a = a.Substring(0, a.IndexOf(","));
                        var b = l.Substring((l.IndexOf(",") + 1));
                        b = b.Substring((b.IndexOf(",") + 1));
                        var c = b.Substring((b.IndexOf(",") + 1));
                        c = c.Substring(0, c.IndexOf("/"));
                        b = b.Substring(0, b.IndexOf(","));
                        w.c = l + Environment.NewLine;
                        for(int i = 1; i < p + 1; i++)
                        {
                            File.AppendAllText(s + @"\split_wire" + i + @".dat", w.c);
                            int e = Count((s + @"\split_wire" + i + @".dat"), g);
                            int d = Count((s + @"\split_wire" + i + @".dat"), h);
                            Replace(a, ts[i - 1], (s + @"\split_wire" + i + @".dat"));
                            Replace(b, e.ToString(), (s + @"\split_wire" + i + @".dat")); 
                            Replace(c, d.ToString(), (s + @"\split_wire" + i + @".dat")); 
                        }
                    } else if(l.Substring(0,2) == "99") //file trailer
                    {
                        string[] h = { "01", "02", "03", "16", "49", "98", "99" };
                        string[] g = { "02" };
                        var a = l.Substring((l.IndexOf(",") + 1));
                        a = a.Substring(0, a.IndexOf(","));
                        var b = l.Substring((l.IndexOf(",") + 1));
                        b = b.Substring((b.IndexOf(",") + 1));
                        var c = b.Substring((b.IndexOf(",") + 1));
                        c = c.Substring(0, c.IndexOf("/"));
                        b = b.Substring(0, b.IndexOf(","));
                        w.d = l + Environment.NewLine;
                        for (int i = 1; i < p + 1; i++)
                        {
                            File.AppendAllText(s + @"\split_wire" + i + @".dat", w.d);
                            int e = Count((s + @"\split_wire" + i + @".dat"), g);
                            int d = Count((s + @"\split_wire" + i + @".dat"), h) + 1;
                            Replace(a, ts[i -1], (s + @"\split_wire" + i + @".dat"));
                            Replace(b, e.ToString(), (s + @"\split_wire" + i + @".dat")); 
                            Replace(c, d.ToString(), (s + @"\split_wire" + i + @".dat")); 
                        }
                    } else if(l.Substring(0,2) == "03") //account identifier & summary status
                    {
                        p++;
                        w.e = 0;
                        l = l + Environment.NewLine;
                        File.AppendAllText(s + @"\split_wire" + p + @".dat", w.a);
                        File.AppendAllText(s + @"\split_wire" + p + @".dat", w.b);
                        File.AppendAllText(s + @"\split_wire" + p + @".dat", l);
                    } else if(l.Substring(0,2) == "16") //transaction detail
                    {
                        var a = l.Substring((l.IndexOf(",") + 1));
                        a = a.Substring((a.IndexOf(",") + 1));
                        a = a.Substring(0, a.IndexOf(","));
                        a = a.Insert(a.Length - 2, ".");
                        decimal b = decimal.Parse(a); //confirmed to be working.
                        w.e = w.e + b;
                        l = l + Environment.NewLine;
                        File.AppendAllText(s + @"\split_wire" + p + @".dat", l);
                    } else if(l.Substring(0,2) == "88") //continuation record
                    {
                        l = l + Environment.NewLine;
                        File.AppendAllText(s + @"\split_wire" + p + @".dat", l);
                    } else if(l.Substring(0,2) == "49")
                    {
                        string[] h = { "03", "16", "88", "49" };
                        var a = l.Substring((l.IndexOf(",") + 1));
                        a = a.Substring(0, a.IndexOf(","));
                        var b = l.Substring((l.IndexOf(",") + 1));
                        b = b.Substring(b.IndexOf(",") + 1);
                        b = b.Substring(0, b.IndexOf('/'));
                        l = l + Environment.NewLine;
                        File.AppendAllText(s + @"\split_wire" + p + @".dat", l);
                        ts.Add(w.e.ToString());
                        int total = Count((s + @"\split_wire" + p + @".dat"), h);
                        Replace(a, w.e.ToString(), (s + @"\split_wire" + p + @".dat"));
                        Replace(b, total.ToString(), (s + @"\split_wire" + p + @".dat"));
                    }
                }
            }
        }
        static void Replace(string x, string y, string file)
        {
            string z = File.ReadAllText(file);
            y = y.Replace(".", ""); //get rid of the decimal point...
            var r = new Regex(@"(?<![\w])" + x + @"(?![\w])", RegexOptions.IgnoreCase);
            z = Regex.Replace(z, r.ToString(), y);
            //Console.WriteLine("Finding " + x + " Replacing with " + y);
            File.WriteAllText(file, z);
        }
        static int Count(string file, string[] arr)
        {
            int total = 0;
            var f = File.ReadLines(file);
            foreach(var l in f)
            {
                foreach(var a in arr)
                {
                    if (l.Substring(0,2).Contains(a))
                    {
                        total++;
                    }
                }
            }
            //Console.WriteLine("The total called: " + total);
            return total;
        }
    }
}
