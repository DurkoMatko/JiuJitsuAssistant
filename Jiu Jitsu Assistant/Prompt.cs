using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jiu_Jitsu_Assistant
{
   public static class Prompt
   {
      public static string ShowDialog(string text, string caption)
      {
         System.Windows.Forms.Form prompt = new System.Windows.Forms.Form()
         {
            Width = 500,
            Height = 150,
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog,
            Text = caption,
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
         };
         System.Windows.Forms.Label textLabel = new System.Windows.Forms.Label() { Left = 50, Top = 20, Text = text };
         System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox() { Left = 50, Top = 50, Width = 400 };
         System.Windows.Forms.Button confirmation = new System.Windows.Forms.Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = System.Windows.Forms.DialogResult.OK };
         confirmation.Click += (sender, e) => { prompt.Close(); };
         prompt.Controls.Add(textBox);
         prompt.Controls.Add(confirmation);
         prompt.Controls.Add(textLabel);
         prompt.AcceptButton = confirmation;

         return prompt.ShowDialog() == System.Windows.Forms.DialogResult.OK ? textBox.Text : "";
      }
   }
}
