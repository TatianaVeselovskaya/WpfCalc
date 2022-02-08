using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using WpfCalc.Models;

namespace WpfCalc.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        //реализация интерфейса INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string PropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        private string lastOperation;       //поле хранящее символ последней нажатой клавиши операции
        private bool newDisplayRequired = false;        //поле хранящее логическое значение что при вводе следующего символа дисплей должен быть предварительно очищен
        private string expression;      //поле хранящее выражение производимых вычислений
        private string display = "0";       //поле хранящее значение отображаемое на дисплее калькулятора

        private CalculatorModel calculator = new CalculatorModel();     //создание экземпляра калькулятора

        public string FirstOperand      //свойство через которое осуществляется взаимодействие с полем FirstOperand модели калькулятора
        {
            get => calculator.FirstOperand;
            set => calculator.FirstOperand = value;
        }

        public string SecondOperand      //свойство через которое осуществляется взаимодействие с полем SecondOperand модели калькулятора
        {
            get => calculator.SecondOperand;
            set => calculator.SecondOperand = value;
        }

        public string Operation      //свойство через которое осуществляется взаимодействие с полем Operation модели калькулятора        
        {
            get => calculator.Operation;
            set => calculator.Operation = value;
        }

        public string LastOperation      //свойство через которое осуществляется взаимодействие с полем lastOperation
        {
            get => lastOperation;
            set => lastOperation = value;
        }

        public string Result      //свойство через которое осуществляется взаимодействие с полем Result 
        {
            get => calculator.Result;
        }

        public string Display      //свойство через которое осуществляется взаимодействие с полем display 
        {
            get
            {
                if (display.Length > 11)
                    return display.Substring(0, 11);
                else
                    return display;
            }
            set
            {
                display = value;
                OnPropertyChanged();
            }
        }

        public string Expression      //свойство через которое осуществляется взаимодействие с полем expression 
        {
            get => expression;
            set
            {
                expression = value;
                OnPropertyChanged();
            }
        }

        public ICommand ClearButtonPressCommand { get; }        //команда нажатия клавиши Clear

        public void OnClearButtonPressCommandExecute(object p)
        {
            Display = "0";
            FirstOperand = string.Empty;
            SecondOperand = string.Empty;
            Operation = string.Empty;
            LastOperation = string.Empty;
            Expression = string.Empty;
            newDisplayRequired = false;
        }

        public bool CanClearButtonPressCommandExecuted(object p)
        {
            return true;
        }

        public ICommand DeleteButtonPressCommand { get; }        //команда нажатия клавиши Delete

        public void OnDeleteButtonPressCommandExecute(object p)
        {
            if (Display.Length > 1)
                Display = Display.Substring(0, Display.Length - 1);
            else
                Display = "0";
        }

        public bool CanDeleteButtonPressCommandExecuted(object p)
        {
            if (Display == "ERROR")
                return false;
            else
                return true;
        }

        public ICommand DigitButtonPressCommand { get; }        //команда нажатия цифровых клавиш

        public void OnDigitButtonPressCommandExecute(object p)
        {
            string button = (string)p;
            switch (button)
            {
                case "+/-":
                    if (Display.Contains("-"))
                        Display = Display.Remove(Display.IndexOf("-"), 1);
                    else
                        Display = "-" + Display;
                    break;
                case ",":
                    if (newDisplayRequired)
                        Display = "0,";
                    else if (!Display.Contains(","))
                        Display += ",";
                    break;
                default:
                    if (Display == "0" || newDisplayRequired)
                        Display = button;
                    else
                        Display += button;
                    break;
            }
            newDisplayRequired = false;
        }

        public bool CanDigitButtonPressCommandExecuted(object p)
        {
            if (Display == "ERROR")
                return false;
            else
                return true;
        }

        public ICommand SingleOperationButtonPressCommand { get; }      //команда нажатия клавиш операций, выполняемых с одним операндом

        public void OnSingleOperationButtonPressCommandExecute(object p)
        {
            string operation = (string)p;
            try
            {
                FirstOperand = Display;
                Operation = operation;
                calculator.CalculateResult();
                Expression = Operation + "(" + Math.Round(Convert.ToDouble(FirstOperand), 5) + ") = ";
                LastOperation = "=";
                Display = Result;
                FirstOperand = Display;
                newDisplayRequired = true;
            }
            catch (Exception)
            {
                Display = "ERROR";
            }
        }

        public bool CanSingleOperationButtonPressCommandExecuted(object p)
        {
            if (Display == "ERROR")
                return false;
            else
                return true;
        }

        public ICommand DoubleOperationButtonPressCommand { get; }      //команда нажатия клавиш операций, выполняемых с двумя операндами

        public void OnDoubleOperationButtonPressCommandExecute(object p)
        {
            string operation = (string)p;
            try
            {
                if (FirstOperand == string.Empty || LastOperation == "=")
                {
                    FirstOperand = Display;
                    LastOperation = operation;
                }
                else
                {
                    SecondOperand = Display;
                    Operation = LastOperation;
                    calculator.CalculateResult();
                    Expression = Math.Round(Convert.ToDouble(FirstOperand), 5) + " "
                        + Operation + " " + Math.Round(Convert.ToDouble(SecondOperand), 5) + " = ";
                    LastOperation = operation;
                    Display = Result;
                    FirstOperand = Display;
                }
                newDisplayRequired = true;
            }
            catch (Exception)
            {
                Display = "ERROR";
            }
        }

        public bool CanDoubleOperationButtonPressCommand(object p)
        {
            if (Display == "ERROR")
                return false;
            else
                return true;
        }

        public MainWindowViewModel()
        {
            //передача управления командами в RelayCommand
            DigitButtonPressCommand = new RelayCommand(OnDigitButtonPressCommandExecute, CanDigitButtonPressCommandExecuted);
            SingleOperationButtonPressCommand = new RelayCommand(OnSingleOperationButtonPressCommandExecute, CanSingleOperationButtonPressCommandExecuted);
            DoubleOperationButtonPressCommand = new RelayCommand(OnDoubleOperationButtonPressCommandExecute, CanDoubleOperationButtonPressCommand);
            ClearButtonPressCommand = new RelayCommand(OnClearButtonPressCommandExecute, CanClearButtonPressCommandExecuted);
            DeleteButtonPressCommand = new RelayCommand(OnDeleteButtonPressCommandExecute, CanDeleteButtonPressCommandExecuted);
        }
    }
}