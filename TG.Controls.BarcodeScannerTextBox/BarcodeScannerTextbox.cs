using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace TG.Controls
{
    /// <summary>
    /// This is a <see cref="TextBox"/> that prevents text being entered from anything other than a bar code reader.
    /// </summary>
    public class BarcodeScannerTextBox : TextBox
    {
        Stopwatch stopWatch = new Stopwatch();
        Timer tmer = new Timer() { Interval = 500 };
        bool keyedIn = false;

        /// <summary>
        /// Creates a new instance of 
        /// </summary>
        public BarcodeScannerTextBox()
        {
            tmer.Tick += Tmer_Tick;

            MaximumKeyDelay = 100;
            TabToNextControlAfterScan = true;
        }

        private void Tmer_Tick(object sender, EventArgs e)
        {
            tmer.Stop();
            stopWatch.Reset();
            keyedIn = false;
            if (TextLength == 1)
                ClearAll();
            else if (TextLength > 0)
            {
                char firstChar = Text[0];
                bool flag = true;
                foreach (char c in Text)
                {
                    if (c != firstChar)
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                    ClearAll();
            }

        }

        /// <summary>
        /// Clears all text and undo data.
        /// </summary>
        public void ClearAll()
        {
            Clear();
            ClearUndo();
        }

        /// <summary>
        /// Resets the entered value to null.
        /// </summary>
        public void ResetEnteredValue()
        {
            EnteredValue = null;
        }

        /// <summary>
        /// Gets or set whether normal text entry is allowed.
        /// </summary>
        [DefaultValue(false)]
        public bool AllowKeyboardEntries { get; set; } = false;

        protected override void OnTextChanged(EventArgs e)
        {
            if (!AllowKeyboardEntries && !keyedIn && TextLength > 0)
            {
                ClearAll();
            }
            base.OnTextChanged(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (!AllowKeyboardEntries)
                EnteredValue += e.KeyChar;
            base.OnKeyPress(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (AllowKeyboardEntries)
                return;
            //Debug.Print(e.KeyCode.ToString() + " - " + stopWatch.ElapsedMilliseconds.ToString());
            if ((e.Modifiers == Keys.Control && e.KeyCode == Keys.V) || e.Handled)
            {
                keyedIn = false;
                return;
            }
            if (e.KeyCode == Keys.Return && TabToNextControlAfterScan)
                this.TopLevelControl?.SelectNextControl(this, true, false, true, true);
            tmer.Stop();
            if (stopWatch.IsRunning)
            {
                if (stopWatch.ElapsedMilliseconds >= MaximumKeyDelay)
                {
                    e.SuppressKeyPress = true;
                    this.ClearAll();
                }
                stopWatch.Reset();
            }
            else
                stopWatch.Start();
            keyedIn = true;
            tmer.Start();

        }

        /// <summary>
        /// The maximum time in milliseconds that is allowed to elapse before the key press is suppressed and the TextBox is cleared.
        /// </summary>
        public int MaximumKeyDelay { get; set; }

        /// <summary>
        /// Gets or Sets if an enter key character the next control in the form.
        /// </summary>
        public bool TabToNextControlAfterScan { get; set; }

        /// <summary>
        /// Gets the entered text.
        /// </summary>
        public string EnteredValue { get; private set; }
    }
}
