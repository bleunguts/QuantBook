using System;
using Caliburn.Micro;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Windows;

namespace QuantBook.Ch01
{
    [Export(typeof(IScreen)), PartCreationPolicy(CreationPolicy.NonShared)]
    public class FirstWpfProgramViewModel : Screen
    {
         [ImportingConstructor]
        public FirstWpfProgramViewModel()
        {
            DisplayName = "01. First WPF Program";            
        }
    }
}
