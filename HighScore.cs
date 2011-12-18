using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace countdown_bug_testing
{
    class HighScore
    {

        struct HS
        {
            public int index;
            public string name;
            public int points;
        }


        HS[] all = new HS[20];
        private string filepath = "highscore.txt";
        bool harvestdata = false;

        public string view_names()
        {
            if (!harvestdata)
                ReadHS();

            string tmp_names = "Name:\n\n";

            for (int i = 1; i <= 10; i++)
                tmp_names += all[i].name + "\n";

                return tmp_names;
        }

        public string view_points()
        {
            if (!harvestdata)
                ReadHS();

            string tmp_names = "Score:\n\n";

            for (int i = 1; i <= 10; i++)
                tmp_names += all[i].points.ToString() + "\n";

            return tmp_names;
        }

        public void add_score(string dude, int thisPoints)
        {

            if (!harvestdata)
                ReadHS();

            bool done = false;

            for (int i = 10; i > 1 && !done; i--)
            {
                if (thisPoints > all[i].points)
                {
                    all[i].name = all[i - 1].name;
                    all[i].points = all[i - 1].points;
                }
                else
                {
                    if (i < 10)
                    {
                        all[i + 1].name = dude;
                        all[i + 1].points = thisPoints;
                        
                    }
                    done = true;
                }
            }

            if (!done && thisPoints > all[1].points)
            {
                all[1].name = dude;
                all[1].points = thisPoints;
            }

            save_score();

        }

        private void save_score()
        {
            TextWriter tw = new StreamWriter(filepath);

            for (int i = 1; i <= 10; i++)
            {
                tw.WriteLine(all[i].name + " " + all[i].points);
            }

            // close the stream
            tw.Close();
        }

        private void ReadHS()
        {

                     harvestdata = true;

                         StreamReader file = new StreamReader(filepath);

                    for (int i = 1; i <= 10; i++)
                    {
                        string line = file.ReadLine();

                        string[] tmp = line.Split(' ');

                        all[i].index = i;
                        all[i].name = tmp[0];
                        all[i].points = Convert.ToInt32(tmp[1]);
                    }

                    file.Close();

            }

        

    }
}
