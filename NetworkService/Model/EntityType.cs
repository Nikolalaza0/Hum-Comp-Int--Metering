﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace NetworkService.Model
{
    public class EntityType : ValidationBase
    {
        private string name;
        private string imgSrc = "pack://application:,,,/NetworkService;component/Images/Empty.png";

        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    ImgSrc = Items.ComboBoxItems.entityTypes[value];
                    OnPropertyChanged("Name");
                }
            }
        }

        public string ImgSrc
        {
            get { return imgSrc; }
            set
            {
                if (imgSrc != value)
                {
                    imgSrc = value;
                    OnPropertyChanged("ImgSrc");
                }
            }
        }

        protected override void ValidateSelf()
        {
            if (this.Name == null)
            {
                this.ValidationErrors["Name"] = "Type must be selected.";
            }
        }
    }
}
