using System;
using VkNet;
using System.Windows.Forms;
using VkNet.Enums.SafetyEnums;
using VkNet.Model.RequestParams;
using VkNet.Enums.Filters;
using VkNet.Model;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace ParserGroupVK
{
    public partial class Form1 : Form
    {

        VkApi _api = new VkApi();
        public Form1()
        {
            InitializeComponent();
        }

        int CountView;
        int CountMembersP;
        int startID;
        int resCheck;

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public string StatuMembersWork(string GroupId)
        {
            var members = _api.Groups.GetById(null, GroupId, GroupsFields.MembersCount);
            
            if (members[0].Name == "DELETED" || members[0].IsClosed.ToString() == "Private" || members[0].Deactivated != null || members[0].IsClosed.ToString() == "Closed")
            {
                return "";
            }
            else
            {
                int countMem = members[0].MembersCount.Value;
                string isClosed = members[0].IsClosed.ToString();
                return countMem.ToString() + ":" + isClosed;
            }
        }
        int chStep;
        public void GetInfoGroup(string GroupId)
        {
            chStep = 3;
            string MemStat = StatuMembersWork(GroupId);

            Thread.Sleep(3000);

            if (MemStat.Length > 0)
            {
                int CountMembers = Int32.Parse(MemStat.Split(':')[0]);
                string isClosed = MemStat.Split(':')[1];
                
                if (isClosed == "Public")
                {
                    try
                    {
                        var get = _api.Wall.Get(new WallGetParams
                        {
                            Filter = WallFilter.Owner,
                            OwnerId = long.Parse("-"+GroupId),
                            Extended = true
                        });

                        Thread.Sleep(3000);

                        if (get != null && get.WallPosts.Count > 0 && CountMembers < CountMembersP)
                        {
                            for (int countPost = 0; countPost < get.WallPosts.Count; countPost++)
                            {
                                if (get.WallPosts[countPost].Views != null && get.WallPosts[countPost].Views.Count > CountView)
                                {
                                    chStep = 1;
                                    break;
                                }
                                else
                                {
                                    chStep = 0;
                                }
                            }
                        }

                        if (chStep == 1)
                        {
                            Log("club" + GroupId + " ПОДХОДИТ");
                        }
                    }
                    catch
                    {

                    }
                    //else if (chStep == 0)
                    //    {
                    //        Log("club" + GroupId + " НЕ подходит");
                    //    }
                    }
                }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            (sender as System.Windows.Forms.Button).Enabled = false;
            auth();

            CountView = Int32.Parse(textBox3.Text);
            CountMembersP = Int32.Parse(textBox2.Text);

            startID = Int32.Parse(textBox4.Text);
            resCheck = Int32.Parse(textBox5.Text);

            int countCheck = resCheck - startID;

            var taskList = new List<Task>();

            for (int i = 0; i < 1; i++)
            {
                taskList.Add(Task.Run(() =>
                {
                    for (int idG = startID; idG < resCheck; idG++)
                    {
                        GetInfoGroup(idG.ToString());
                        label1.BeginInvoke(new InvokeDelegate(StatusWork));
                        statusWork = "Осталось пройти: " + countCheck--.ToString();

                        //WriteFile(mutex, idG);
                    }
                }));
            }

            await Task.WhenAll(taskList);
            (sender as System.Windows.Forms.Button).Enabled = true;
        }

        public void Log(string value)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.Invoke(new Action<string>(Log), value);
                return;
            }
            textBox1.Text += value + Environment.NewLine;
        }

        public void auth()
        {
            string email = "leva.kekish@mail.ru";
            string pass = "iVK_8973!";
            _api.Authorize(new ApiAuthParams
            {
                ApplicationId = 6921307,
                Login = email,
                Password = pass,
                Settings = Settings.All
            });
        }

        public delegate void InvokeDelegate();
        string statusWork;
        public void StatusWork()
        {
            label1.Text = statusWork;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        object mutex = new object();
        public void WriteFile(object mutex, int value)
        {
            lock (mutex)
            {
                using (var fs = new FileStream(@"D:/id.txt", FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    using (var sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(value.ToString());
                    }
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
