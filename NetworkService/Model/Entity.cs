using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using static System.Net.Mime.MediaTypeNames;

namespace NetworkService.Model
{
    public class Entity : ValidationBase
    {
        private string textId;

        private int id;
        private string name;
        private EntityType type;
        private double value;

        public int Id
        {
            get { return id; }
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged("Id");
                }
            }
        }

        public string TextId
        {
            get { return textId; }
            set
            {
                if (textId != value)
                {
                    textId = value;
                    OnPropertyChanged("TextId");
                }
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public EntityType Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    type = value;
                    Type.Name = value.Name;
                    Type.ImgSrc = value.ImgSrc;
                    OnPropertyChanged("Type");
                }
            }
        }

        public double Value
        {
            get { return this.value; }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    OnPropertyChanged("Value");
                }
            }
        }

        public bool IsValueValidForType()
        {
            bool isValid = true;
            if(Value > 5 || Value < 1)
                isValid = false;

            return isValid;
        }

        public bool IsSolar()
        {
            if(Type.Name == "Solar")
                return true;
            return false;
        }

        protected override void ValidateSelf()
        {
            int tempId;
            bool parsingSuccess = int.TryParse(this.textId, out tempId);

            if (this.DoesIdAlreadyExist)
            {
                this.ValidationErrors["Id"] = "Id already exists.";
            }

            if (!parsingSuccess)
            {
                this.ValidationErrors["Id"] = "Id must be an integer.";
            }
            else if (tempId < 0)
            {
                this.ValidationErrors["Id"] = "Id can't be negative.";
            }

            if (string.IsNullOrWhiteSpace(this.textId))
            {
                this.ValidationErrors["Id"] = "Id is required.";
            }

            if (string.IsNullOrWhiteSpace(this.name))
            {
                this.ValidationErrors["Name"] = "Name is required.";
            }
        }
    }

    //public class EntityByExtension
    //{
    //    public string Extension { get; set; }

    //    public BindingList<Entity> EBE { get; set; }

    //    public EntityByExtension()
    //    {

    //        EBE = new BindingList<Entity>();
    //    }
    //}

}
