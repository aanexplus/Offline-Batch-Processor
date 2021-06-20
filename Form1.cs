using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Offline_Batch_Processor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
         
            fbd.Description = "Select the Participant Folder";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // Explode the selectedPath to get the last array

                string selectedpath = fbd.SelectedPath;
                DirectoryInfo info = new DirectoryInfo(selectedpath);
                string[] words = selectedpath.Split('\\');
                string foldername = words[words.Length - 1];
                //String.splselectedpath
                label1.Visible = true;
                label1.Text = "You are working on the " + foldername + " folder";

                //Get Path of the Offline Processing Code
                string relPath = "offline_main/offline_processor.exe";
                string absPath = Path.GetFullPath(relPath);

                //Get all the mkv files in the selected folder and inject file name into the offline processing code
                FileInfo[] files = info.GetFiles("*.mkv");
                string str = "";

                foreach(FileInfo file in files)
                {
                    //cmd usage for offline_processor.exe is offline_processor.exe mkv_file_to_process.mkv preferred_output_json_file_name.json
                    string modifiedpath = selectedpath + "\\" + file.Name;
                    string jsonoutput = selectedpath + "\\" + file.Name + ".json";
                    Process exeProcess = Process.Start(absPath,$"{modifiedpath} {jsonoutput}");
                    str = selectedpath + "\\" + file.Name + ", " + str ;
                    exeProcess.WaitForExit();
                }

                MessageBox.Show("Processing Completed");
            }
                
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select the Participant Folder";
            List<string> procData = new List<string> ();

            //StreamReader 

            if(fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string selectedpath = fbd.SelectedPath;
                DirectoryInfo info = new DirectoryInfo(selectedpath);
                string[] words = selectedpath.Split('\\');
                string Positionfoldername = selectedpath + "\\Position Data";
                string Orientationfoldername = selectedpath + "\\Orientation Data";

                //Create Folders
                System.IO.Directory.CreateDirectory(Positionfoldername);
                System.IO.Directory.CreateDirectory(Orientationfoldername);

                string foldername = words[words.Length - 1];
                //String.splselectedpath
                label1.Visible = true;
                label1.Text = "You are working on the " + foldername + " folder";

                //Get all the JSON files in the selected folder and inject file name into the offline processing code
                FileInfo[] files = info.GetFiles("*.json");
                string str = "";

                foreach (FileInfo file in files)
                {
                    StreamReader ffile = File.OpenText(selectedpath + "\\" + file.Name);
                    StreamWriter writer = new StreamWriter(Positionfoldername + "\\" + file.Name + ".txt");
                    StreamWriter orientWriter = new StreamWriter(Orientationfoldername + "\\" + file.Name + ".txt");
                    JsonTextReader reader = new JsonTextReader(ffile);
                    JObject txt = (JObject)JToken.ReadFrom(reader);
                    JArray items = (JArray)txt["frames"];

                    for(int i = 0; i < items.Count; i++)
                    {
                        if(items[i]["bodies"].Count() > 0)
                        {
                            writer.Write("frame-" + i + "\t" + items[i]["timestamp_usec"] + "\t");
                            for (int j = 0; j< items[i]["bodies"][0]["joint_positions"].Count(); j++)
                            {
                                writer.Write(items[i]["bodies"][0]["joint_positions"][j][0] + "\t" + items[i]["bodies"][0]["joint_positions"][j][1] + "\t" + items[i]["bodies"][0]["joint_positions"][j][2] + "\t");
                            }
                            writer.WriteLine();
                        }
                    }

                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i]["bodies"].Count() > 0)
                        {
                            for (int j = 0; j < items[i]["bodies"][0]["joint_positions"].Count(); j++)
                            {
                                orientWriter.Write(items[i]["bodies"][0]["joint_orientations"][j][0] + "\t" + items[i]["bodies"][0]["joint_orientations"][j][1] + "\t" + items[i]["bodies"][0]["joint_orientations"][j][2] + "\t" + items[i]["bodies"][0]["joint_orientations"][j][3] + "\t");
                            }
                            orientWriter.WriteLine();
                        }
                    }
                }
                MessageBox.Show("Processing Completed");

            }
        }
    }
}
