using System;
using GuestListManager.Data;
using Prism.Commands;
using Prism.Mvvm;

namespace GuestListManager.ViewModels
{
    public class GuestEnrolmentVewModel : BindableBase
    {
        private string lastName;

        public string LastName
        {
            get
            {
                return lastName;
            }
            set
            {
                SetProperty(ref lastName, value);
            }
        }

        private string firstName;

        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {
                SetProperty(ref firstName, value);
            }
        }


        private DelegateCommand addGuestCommand;

        public DelegateCommand AddGuestCommand
        {
            get
            {
                if (addGuestCommand == null)
                    addGuestCommand = new DelegateCommand(AddGuestCommandExecute);

                return addGuestCommand;
            }
        }

        private void AddGuestCommandExecute()
        {
            Guest guest = new Guest();
            guest.Id = Guid.NewGuid().ToString();
            guest.Name = LastName + ", " + FirstName;
        }
    }
}
