using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace NetworkService.ViewModel
{
    public class MeasurementGraphViewModel : BindableBase
    {
        public static ObservableCollection<Entity> Entities { get; set; } = new ObservableCollection<Entity>();
        public List<string> ComboBoxItems { get; set; } = Items.ComboBoxItems.entityTypes.Keys.ToList();


        public MeasurementGraphViewModel()
        {
        }
    }
}
