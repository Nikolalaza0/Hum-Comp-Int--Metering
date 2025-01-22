using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Controls;

namespace NetworkService.ViewModel
{
    public class NetworkEntitiesViewModel : BindableBase
    {
        public List<string> ComboBoxItems { get; set; } = Items.ComboBoxItems.entityTypes.Keys.ToList();

        public ObservableCollection<Entity> EntitiesToShow { get; set; }
        public ObservableCollection<Entity> Entities { get; set; }
        public ObservableCollection<Entity> FilteredEntities { get; set; }
        public static ObservableCollection<Entity> BackUpEntities { get; private set; } = new ObservableCollection<Entity>();

        public MyICommand<TextBox> gotFocusId { get; set; }
        public MyICommand<TextBox> gotFocusFilter { get; set; }
        public MyICommand<TextBox> gotFocusName { get; set; }

        public MyICommand<string> buttonPress { get; set; }
        public MyICommand buttonDelete { get; set; }

        public MyICommand buttonOut { get; set; }
        public MyICommand buttonDeleteAll { get; set; }
        public MyICommand buttonEnter { get; set; }

        public MyICommand Refresh { get; set; }
        public MyICommand AddEntityCommand { get; set; }
        public MyICommand DeleteEntityCommand { get; set; }
        public MyICommand FilterEntityCommand { get; set; }
        public MyICommand ResetFilterCommand { get; set; }

        // Unos novog entiteta
        private Entity currentEntity = new Entity();
        private EntityType currentEntityType = new EntityType();

        // Entitet selektovan u datagridu
        private Entity selectedEntity;

        // Filtriranje
        private string selectedEntityTypeToFilter;
        private bool isGreaterThanRadioButtonSelected;
        private bool isLessThanRadioButtonSelected;
        private string selectedIdMarginToFilterText;
        private bool isOutOfRangeValuesRadioButtonSelected;
        private bool isExpectedValuesRadioButtonSelected;
        private string filterErrorMessage;

        public NetworkEntitiesViewModel()
        {
            Entities = new ObservableCollection<Entity>();
            EntitiesToShow = Entities;
            Refresh = new MyICommand(OnRefresh);

            LoadDefaultValuesForDisplay();
            AddEntityCommand = new MyICommand(OnAdd);
            DeleteEntityCommand = new MyICommand(OnDelete, CanDelete);
            FilterEntityCommand = new MyICommand(OnFilter);
            ResetFilterCommand = new MyICommand(OnResetFilter);

            buttonPress = new MyICommand<string>(OnbuttonPress);
            buttonDelete = new MyICommand(OnbuttonDelete);

            buttonOut = new MyICommand(OnbuttonOut);

            buttonDeleteAll = new MyICommand(OnbuttonDeleteAll);
            buttonEnter = new MyICommand(OnbuttonEnter);

            gotFocusId = new MyICommand<TextBox>(OngotFocusId);
            gotFocusName = new MyICommand<TextBox>(OngotFocusName);
            gotFocusFilter = new MyICommand<TextBox>(OngotFocusFilter);
            KeyboardVisible = Visibility.Collapsed;
            OnRefresh();
        }

        private void OnRefresh()
        {
            //Entities.Clear();
            //foreach (Entity entity1 in BackUpEntities)
            //{
            //    Entities.Add(entity1);
            //}

        }

        private void LoadDefaultValuesForDisplay()
        {
        }

        public Entity SelectedEntity
        {
            get { return selectedEntity; }
            set
            {
                selectedEntity = value;
                DeleteEntityCommand.RaiseCanExecuteChanged();
            }
        }

        public string SelectedEntityTypeToFilter
        {
            get { return selectedEntityTypeToFilter; }
            set
            {
                selectedEntityTypeToFilter = value;
                OnPropertyChanged("SelectedEntityTypeToFilter");
            }
        }

        //public bool IsGreaterThanRadioButtonSelected
        //{
        //    get { return isGreaterThanRadioButtonSelected; }
        //    set
        //    {
        //        isGreaterThanRadioButtonSelected = value;
        //        OnPropertyChanged("IsGreaterThanRadioButtonSelected");
        //    }
        //}

        //public bool IsLessThanRadioButtonSelected
        //{
        //    get { return isLessThanRadioButtonSelected; }
        //    set
        //    {
        //        isLessThanRadioButtonSelected = value;
        //        OnPropertyChanged("IsLessThanRadioButtonSelected");
        //    }
        //}

        public string SelectedIdMarginToFilterText
        {
            get { return selectedIdMarginToFilterText; }
            set
            {
                selectedIdMarginToFilterText = value;
                OnPropertyChanged("SelectedIdMarginToFilterText");
            }
        }

        //here
        public bool IsOutOfRangeValuesRadioButtonSelected
        {
            get { return isOutOfRangeValuesRadioButtonSelected; }
            set
            {
                isOutOfRangeValuesRadioButtonSelected = value;
                OnPropertyChanged("IsOutOfRangeValuesRadioButtonSelected");
            }
        }

        //here
        public bool IsExpectedValuesRadioButtonSelected
        {
            get { return isExpectedValuesRadioButtonSelected; }
            set
            {
                isExpectedValuesRadioButtonSelected = value;
                OnPropertyChanged("IsExpectedValuesRadioButtonSelected");
            }
        }

        public string FilterErrorMessage
        {
            get { return filterErrorMessage; }
            set
            {
                filterErrorMessage = value;
                OnPropertyChanged("FilterErrorMessage");
            }
        }

        private void OnFilter()
        {
            FilterErrorMessage = String.Empty;

            if (SelectedEntityTypeToFilter == null &&
                //!IsGreaterThanRadioButtonSelected &&
                //!IsLessThanRadioButtonSelected &&
                string.IsNullOrWhiteSpace(SelectedIdMarginToFilterText) &&
                !IsOutOfRangeValuesRadioButtonSelected &&
                !IsExpectedValuesRadioButtonSelected)
            {
                FilterErrorMessage = "Fields can't be empty.";
                return;
            }

            List<Entity> filteredList = new List<Entity>();

            foreach (Entity entity in Entities)
            {
                filteredList.Add(entity);
            }

            if (IsOutOfRangeValuesRadioButtonSelected)
            {
                List<Entity> entitiesToDelete = new List<Entity>();


                if (string.IsNullOrWhiteSpace(SelectedIdMarginToFilterText))
                {
                    FilterErrorMessage = "Text is required.";
                }

                for (int i = 0; i < filteredList.Count; i++)
                {
                    if (!filteredList[i].Name.ToLower().Contains(SelectedIdMarginToFilterText.ToLower()))
                    {
                        entitiesToDelete.Add(filteredList[i]);
                    }
                }

                foreach (Entity entity in entitiesToDelete)
                {
                    filteredList.Remove(entity);
                }
            }

            //Gotov
            else if (IsExpectedValuesRadioButtonSelected)
            {
                List<Entity> entitiesToDelete = new List<Entity>();

                if (string.IsNullOrWhiteSpace(SelectedIdMarginToFilterText))
                {
                    FilterErrorMessage = "Text is required.";
                }

                for (int i = 0; i < filteredList.Count; i++)
                {
                    if (!filteredList[i].Type.Name.ToLower().Contains(SelectedIdMarginToFilterText.ToLower()))
                    {
                        entitiesToDelete.Add(filteredList[i]);
                    }
                }

                foreach (Entity entity in entitiesToDelete)
                {
                    filteredList.Remove(entity);
                }
            }

            if (filteredList.Count > 0)
            {
                FilteredEntities = new ObservableCollection<Entity>();

                foreach (Entity entity in filteredList)
                {
                    FilteredEntities.Add(entity);
                }

                EntitiesToShow = FilteredEntities;
                OnPropertyChanged("EntitiesToShow");
            }
            else
            {
                FilterErrorMessage = "No entities to show.";
                EntitiesToShow = Entities;
                OnPropertyChanged("EntitiesToShow");
            }
        }



        private void OnResetFilter()
        {
            SelectedEntityTypeToFilter = null;
            //IsGreaterThanRadioButtonSelected = false;
            //IsLessThanRadioButtonSelected = false;
            SelectedIdMarginToFilterText = String.Empty;
            IsOutOfRangeValuesRadioButtonSelected = false;
            IsExpectedValuesRadioButtonSelected = false;
            FilterErrorMessage = String.Empty;

            EntitiesToShow = Entities;
            FilteredEntities = new ObservableCollection<Entity>();
            OnPropertyChanged("EntitiesToShow");
        }

        //        //        //        //        //        //        //        //        //        //        //        //

        private Visibility keyboardVisible;
        private string valueChanged;

        public Visibility KeyboardVisible
        {
            get { return keyboardVisible; }
            set
            {
                if (keyboardVisible != value)
                {
                    keyboardVisible = value;
                    OnPropertyChanged("KeyboardVisible");
                }
            }
        }

        private void OngotFocusId(TextBox textBox)
        {
            KeyboardVisible = Visibility.Visible;
            WriteTextBox = textBox;
            Input = textBox.Text;
            valueChanged = "Id";


        }

        private void OngotFocusName(TextBox textBox)
        {
            KeyboardVisible = Visibility.Visible;
            WriteTextBox = textBox;
            Input = textBox.Text;
            valueChanged = "Name";
        }
        private void OngotFocusFilter(TextBox textBox)
        {
            KeyboardVisible = Visibility.Visible;
            WriteTextBox = textBox;
            Input = textBox.Text;
            valueChanged = "Filter";
        }

        //        //        //        //        //        //        //        //        //        //        //        //


        private void OnbuttonPress(string button)
        {
            Input += button;
        }

        private void OnbuttonDelete()
        {
            if (Input.Count() > 0)
            {
                Input = Input.Remove(Input.Length - 1);
            }

        }

        private void OnbuttonOut()
        {
            KeyboardVisible = Visibility.Collapsed;

        }

        private void OnbuttonDeleteAll()
        {
            Input = "";
        }

        private void OnbuttonEnter()
        {
            if (valueChanged != null)
            {
                if (valueChanged.Equals("Id"))
                {
                    CurrentEntity.TextId = Input;
                    //Add.RaiseCanExecuteChanged();
                }
                else if (valueChanged.Equals("Name"))
                {
                    CurrentEntity.Name = Input;
                    //Add.RaiseCanExecuteChanged();
                }
                else if (valueChanged.Equals("Filter"))
                {
                    SelectedIdMarginToFilterText = Input;
                    //Filter.RaiseCanExecuteChanged();
                }
            }
            WriteTextBox.Text = Input;
            Input = "";
            KeyboardVisible = Visibility.Collapsed;
        }

        public static string InputText { get; set; }
        public static TextBox WriteTextBox { get; set; } = new TextBox();

        private string input;
        public string Input
        {
            get { return input; }
            set
            {
                if (input != value)
                {
                    input = value;
                    OnPropertyChanged("Input");
                }
            }
        }
        


        public void UndoDelete(Entity entity)
        {
            Entities.Add(entity);
            BackUpEntities.Add(entity);
            OnDataGridUpdate();
            OnRefresh();

        }

        public void UndoAdd(Entity entity)
        {
            Entities.Remove(entity);
            BackUpEntities.Remove(entity);
            OnDataGridUpdate();
            OnRefresh();

        }

        private bool CanDelete()
        {
            return SelectedEntity != null;
        }

        private void OnDelete()
        {

            MainWindowViewModel.LastAction = selectedEntity;
            MainWindowViewModel.LastActionId = Actions.DELETE;
            Entities.Remove(SelectedEntity);
            BackUpEntities.Remove(SelectedEntity);
            OnDataGridUpdate();
            OnRefresh();
        }

        public Entity CurrentEntity
        {
            get { return currentEntity; }
            set
            {
                currentEntity = value;
                OnPropertyChanged("CurrentEntity");
            }
        }

        public EntityType CurrentEntityType
        {
            get { return currentEntityType; }
            set
            {
                currentEntityType = value;
                OnPropertyChanged("CurrentEntityType");
            }
        }

        private void OnDataGridUpdate()
        {

        }

        public void OnAdd()
        {
            try
            {
                int parsedId;
                bool canParse = int.TryParse(CurrentEntity.TextId, out parsedId);
                bool idAlreadyExists = false;

                if (canParse)
                {
                    foreach (Entity entity in Entities)
                    {
                        if (entity.Id == parsedId)
                        {
                            idAlreadyExists = true;
                            break;
                        }
                    }
                }

                CurrentEntity.DoesIdAlreadyExist = idAlreadyExists;

                CurrentEntity.Validate();
                CurrentEntityType.Validate();

                if (CurrentEntity.IsValid)
                {
                    Entities.Add(new Entity()
                    {
                        Id = int.Parse(CurrentEntity.TextId),
                        Name = CurrentEntity.Name,
                        Type = new EntityType
                        {
                            Name = CurrentEntityType.Name,
                            ImgSrc = CurrentEntityType.ImgSrc
                        },
                        Value = 0
                    });

                    OnDataGridUpdate();

                    CurrentEntity.TextId = String.Empty;
                    CurrentEntity.Name = String.Empty;
                    BackUpEntities.Add(CurrentEntity);
                    MainWindowViewModel.LastAction = CurrentEntity;
                    MainWindowViewModel.LastActionId = Actions.ADD;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} - {ex.Message}");
            }
        }
    }
}
