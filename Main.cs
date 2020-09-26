using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace DiscordStreamKit
{
    public partial class Main : Form
    {

        public List<DiscordMember> discordMembers;

        public Main()
        {
            InitializeComponent();
            discordMembers = new List<DiscordMember>();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            tbAvatarURL.Enabled = checkBox1.Checked;
            btnLoadAvatar.Enabled = checkBox1.Checked;
        }

        private void btnLoadAvatar_Click(object sender, EventArgs e)
        {
            pictureBox1.Load(tbAvatarURL.Text);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            tbSpeakingURL.Enabled = checkBox2.Checked;
            btnSpeakingLoad.Enabled = checkBox2.Checked;
        }

        private void btnSpeakingLoad_Click(object sender, EventArgs e)
        {
            pictureBox2.Load(tbSpeakingURL.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(tbUserName.Text))
            {
                MessageBox.Show("User Name cannot be left blank!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if(string.IsNullOrWhiteSpace(tbUserID.Text))
            {
                MessageBox.Show("User ID cannot be left blank!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if(checkBox1.Checked && string.IsNullOrWhiteSpace(tbAvatarURL.Text))
            {
                MessageBox.Show("Avatar URL cannot be left blank!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (checkBox2.Checked && string.IsNullOrWhiteSpace(tbSpeakingURL.Text))
            {
                MessageBox.Show("Speaking URL cannot be left blank!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DiscordMember member = new DiscordMember()
            {
                readableName = tbUserName.Text,
                userID = tbUserID.Text,
                avatarURL = checkBox1.Checked ? tbAvatarURL.Text : string.Empty,
                speakingURL = checkBox2.Checked ? tbSpeakingURL.Text : string.Empty
            };

            if(discordMembers.Contains(member))
            {
                MessageBox.Show("Member already exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            discordMembers.Add(member);
            listBox1.Items.Add(tbUserName.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index < 0 || index >= listBox1.Items.Count) return;


            DiscordMember member = discordMembers[index];
            member.readableName = tbUserName.Text;
            member.userID = tbUserID.Text;
            member.avatarURL = checkBox1.Checked ? tbAvatarURL.Text : string.Empty;
            member.speakingURL = checkBox2.Checked ? tbSpeakingURL.Text : string.Empty;

            discordMembers[index] = member;
            listBox1.Items[index] = tbUserName.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index < 0 || index >= listBox1.Items.Count) return;

            discordMembers.RemoveAt(index);
            listBox1.Items.RemoveAt(index);

            index--;
            if (index < 0) index = 0;
            if (listBox1.Items.Count != 0) listBox1.SelectedIndex = index;

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = listBox1.SelectedIndex;
            if (index < 0 || index >= listBox1.Items.Count) return;
            DiscordMember member = discordMembers[index];
            tbUserName.Text = member.readableName;
            tbUserID.Text = member.userID;
            if(!string.IsNullOrEmpty(member.avatarURL))
            {
                checkBox1.Checked = true;
                tbAvatarURL.Text = member.avatarURL;
            }
            if (!string.IsNullOrEmpty(member.speakingURL))
            {
                checkBox2.Checked = true;
                tbSpeakingURL.Text = member.speakingURL;
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            string urlFormat = tbFormatURL.Text;
            urlFormat = urlFormat.Replace("SERVER_ID", tbServerID.Text);
            urlFormat = urlFormat.Replace("CHANNEL_ID", tbChannelID.Text);

            tbOutputURL.Text = urlFormat;

            string css = "";

            string stateFormat = tbVoiceStateFormat.Text;
            stateFormat = stateFormat.Replace("YOUR_ID", tbYourID.Text);

            css += stateFormat;

            css += "\n\n";
            
            foreach(DiscordMember member in discordMembers)
            {
                if (string.IsNullOrEmpty(member.avatarURL)) continue;

                string avatarFormat = tbAvatarFormat.Text;
                avatarFormat = avatarFormat.Replace("THEIR_ID", member.userID);
                avatarFormat = avatarFormat.Replace("THEIR_NONSPEAKING_IMAGE_URL", member.avatarURL);

                css += avatarFormat;
                css += "\n";
            }
            css += "\n";
            foreach(DiscordMember member in discordMembers)
            {
                if (string.IsNullOrEmpty(member.speakingURL)) continue;

                string speakingFormat = tbSpeakingFormat.Text;
                speakingFormat = speakingFormat.Replace("THEIR_ID", member.userID);
                speakingFormat = speakingFormat.Replace("THEIR_SPEAKING_IMAGE_URL", member.speakingURL);

                css += speakingFormat;
                css += "\n";
            }
            css += "\n";
            css += rtbCSSTemplate.Text;

            rtbCSSOutput.Text = css;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(toolStripStatusLabel1.Text == "No file open")
            {
                DialogResult result = saveDialog.ShowDialog();
                if(result == DialogResult.OK)
                {
                    toolStripStatusLabel1.Text = saveDialog.FileName;
                    Save();
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = saveDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                toolStripStatusLabel1.Text = saveDialog.FileName;
                Save();
            }
        }


        private void Save()
        {
            string filename = toolStripStatusLabel1.Text;

            using(BinaryWriter writer = new BinaryWriter(File.Open(filename, FileMode.OpenOrCreate)))
            {
                writer.Write(tbServerID.Text);
                writer.Write(tbChannelID.Text);
                writer.Write(tbYourID.Text);
                writer.Write(discordMembers.Count);
                for(var i = 0; i < discordMembers.Count; i++)
                {
                    var member = discordMembers[i];
                    writer.Write(member.readableName);
                    writer.Write(member.userID);
                    writer.Write(member.avatarURL);
                    writer.Write(member.speakingURL);
                }
                writer.Write(tbFormatURL.Text);
                writer.Write(tbVoiceStateFormat.Text);
                writer.Write(tbAvatarFormat.Text);
                writer.Write(tbSpeakingFormat.Text);
                writer.Write(rtbCSSTemplate.Text);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if(result == DialogResult.OK)
            {
                toolStripStatusLabel1.Text = openFileDialog1.FileName;
            }

            string filename = toolStripStatusLabel1.Text;

            using (BinaryReader reader = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                tbServerID.Text = reader.ReadString();
                tbChannelID.Text = reader.ReadString();
                tbYourID.Text = reader.ReadString();
                int members = reader.ReadInt32();
                discordMembers.Clear();
                listBox1.Items.Clear();
                for (var i = 0; i < members; i++)
                {
                    DiscordMember member = new DiscordMember();
                    member.readableName = reader.ReadString();
                    member.userID = reader.ReadString();
                    member.avatarURL = reader.ReadString();
                    member.speakingURL = reader.ReadString();

                    discordMembers.Add(member);
                    listBox1.Items.Add(member.readableName);
                }

                tbFormatURL.Text = reader.ReadString();
                tbVoiceStateFormat.Text = reader.ReadString();
                tbAvatarFormat.Text = reader.ReadString();
                tbSpeakingFormat.Text = reader.ReadString();
                rtbCSSTemplate.Text = reader.ReadString();
            }

            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(rtbCSSOutput.Text);
        }
    }

    public struct DiscordMember
    {
        public string readableName;
        public string userID;
        public string avatarURL;
        public string speakingURL;
    }
}
