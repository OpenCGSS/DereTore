using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DereTore.Applications.StarlightDirector.Entities;

namespace DereTore.Applications.StarlightDirector.UI.Controls.Primitives
{
    /// <summary>
    /// Interaction logic for SimpleScoreNote.xaml
    /// </summary>
    public partial class SimpleScoreNote : UserControl
    {
        public static readonly DependencyProperty NoteProperty = DependencyProperty.Register("Note", typeof(Note), typeof(SimpleScoreNote), new PropertyMetadata(default(Note)));

        public SimpleScoreNote()
        {
            InitializeComponent();
        }

        public Note Note
        {
            get { return (Note) GetValue(NoteProperty); }
            set { SetValue(NoteProperty, value); }
        }
    }
}
