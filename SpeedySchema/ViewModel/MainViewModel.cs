using GalaSoft.MvvmLight;
using SpeedySchema.Engine;
using System.Collections.ObjectModel;

namespace SpeedySchema.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public QueryParser eng { get; set; }
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                this.AppTitle = "Speedy Schema - DESIGN";
                this.InputMessage = "::Contact>";
                this.SQLStatement = "CREATE TABLE Contact ()";
            }
            else
            {
                // Code runs "for real"
                this.AppTitle = "Speedy Schema";

                if (eng == null) eng = new QueryParser();

                this.InputMessage = "";
                this.SQLStatement = "";

                this.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == "InputMessage")
                    {
                        this.SQLStatement = eng.Parse(this._input);
                    }
                };

#if DEBUG
                this.InputMessage = "::Contact>*Id, Firstname, LastName, DOB(DateTime?), Age(int), Middlename(text[10]?)";
#else
                this.InputMessage = "";
#endif
            }
        }

        public ObservableCollection<TableQuery> Queries { get; set; }

        private string _apptitle;
        public string AppTitle
        {
            get { return _apptitle; }
            set
            {
                if (_apptitle == value) return;
                _apptitle = value;
                RaisePropertyChanged("AppTitle");
            }
        }

        private string _input;
        public string InputMessage
        {
            get { return _input; }
            set
            {
                if (_input == value) return;
                _input = value;
                RaisePropertyChanged("InputMessage");
            }
        }

        private string _SqlStatement;
        public string SQLStatement
        {
            get { return _SqlStatement; }
            set
            {
                if (_SqlStatement == value) return;
                _SqlStatement = value;
                RaisePropertyChanged("SQLStatement");
            }
        }
    }
}