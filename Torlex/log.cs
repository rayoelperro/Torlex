using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Torlex
{
    public partial class log : Form
    {
        public log(string content)
        {
            InitializeComponent();
            richTextBox1.ReadOnly = true;
            richTextBox1.Text = content;
        }
    }
}
