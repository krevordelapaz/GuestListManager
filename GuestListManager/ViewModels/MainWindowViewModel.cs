using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using GuestListManager.Data;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Data;

namespace GuestListManager.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        AmazonDynamoDBClient client;
        Table guestsTable;

        private string filter;

        public string Filter
        {
            get
            {
                return filter;
            }
            set
            {
                SetProperty(ref filter, value);
                GuestsListView.Refresh();
            }
        }


        public ObservableCollection<Guest> Guests
        {
            get;
            set;
        }

        private Guest selectedGuest;

        public Guest SelectedGuest
        {
            get
            {
                return selectedGuest;
            }
            set
            {
                SetProperty(ref selectedGuest, value);
                CheckInGuestCommand.RaiseCanExecuteChanged();
            }
        }

        private Visibility addGuestVisibility;

        public Visibility AddGuestVisibility
        {
            get
            {
                return addGuestVisibility;
            }
            set
            {
                SetProperty(ref addGuestVisibility, value);
            }
        }

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

        public ICollectionView GuestsListView
        {
            get;
            private set;
        }

        private DelegateCommand checkInGuestCommand;

        public DelegateCommand CheckInGuestCommand
        {
            get
            {
                if (checkInGuestCommand == null)
                {
                    checkInGuestCommand = new DelegateCommand(CheckInGuestCommandExecute, CanCheckInGuest);
                }
                return checkInGuestCommand;
            }
        }

        private DelegateCommand uploadGuestListCommand;

        public DelegateCommand UploadGuestListCommand
        {
            get
            {
                if (uploadGuestListCommand == null)
                {
                    uploadGuestListCommand = new DelegateCommand(UploadGuestListCommandExecute, CanUploadGuestList);
                }
                return uploadGuestListCommand;
            }
        }

        private DelegateCommand enableAddGuestCommand;

        public DelegateCommand EnableAddGuestCommand
        {
            get
            {
                if(enableAddGuestCommand == null)
                {
                    enableAddGuestCommand = new DelegateCommand(EnableAddGuestCommandExecute);
                }
                return enableAddGuestCommand;
            }
        }

        private DelegateCommand cancelAddGuestCommand;

        public DelegateCommand CancelAddGuestCommand
        {
            get
            {
                if(cancelAddGuestCommand == null)
                {
                    cancelAddGuestCommand = new DelegateCommand(CancelAddGuestCommandExecute);
                }
                return cancelAddGuestCommand;
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
            try
            {

                var items = Guests.Where(item => item.Name.ToLower() == LastName.ToLower() + ", " + FirstName.ToLower());

                if(items.Count() == 0)
                {
                    Guest guest = new Guest()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = LastName + ", " + FirstName,
                        HasFan = false,
                        DesignatedSeats = "1-2",
                        CheckInTime = DateTime.Now.ToLocalTime().ToString(),
                        IsCheckedIn = true
                    };

                    var request = new PutItemRequest
                    {
                        TableName = "TABLE_Guests",
                        Item = new Dictionary<string, AttributeValue>()
                        {
                            {"id", new AttributeValue { S = guest.Id } },
                            { "Name", new AttributeValue { S = guest.Name }},
                            { "HasFan", new AttributeValue { BOOL = guest.HasFan } },
                            { "DesignatedSeats", new AttributeValue { S = guest.DesignatedSeats } }, //TODO:
                            { "CheckInTime", new AttributeValue { S = guest.CheckInTime } },
                            { "IsCheckedIn", new AttributeValue { BOOL = guest.IsCheckedIn } }
                        }
                    };

                    client.PutItem(request);

                    Guests.Add(guest);
                    SelectedGuest = guest;
                    OnPropertyChanged("Guests");
                    AddGuestVisibility = Visibility.Hidden;

                    LastName = string.Empty;
                    FirstName = string.Empty;
                }
                else
                {
                    MessageBox.Show("Duplicate Guest.");
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public MainWindowViewModel()
        {
            Guests = new ObservableCollection<Guest>();
            ConnectDatabase();
            GuestsListView = CollectionViewSource.GetDefaultView(Guests);
            GuestsListView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            GuestsListView.Filter += GuestFilter;
            AddGuestVisibility = Visibility.Hidden;
        }

        private void EnableAddGuestCommandExecute()
        {
            AddGuestVisibility = Visibility.Visible;
        }

        private void CancelAddGuestCommandExecute()
        {
            AddGuestVisibility = Visibility.Hidden;
        }

        private bool GuestFilter(object item)
        {
            Guest guest = item as Guest;

            if(Filter != null)
            {
                if (guest.Name.ToLower().Contains(Filter.ToLower()))
                    return true;
                else
                    return false;
            }
            else
            {
                return true;
            }

        }

        /// <summary>
        /// Creates table
        /// </summary>
        private CreateTableResponse CreateGuestTable()
        {
            CreateTableRequest createRequest = new CreateTableRequest
            {
                TableName = "TABLE_Guests",
                AttributeDefinitions = new List<AttributeDefinition>()
                {
                    new AttributeDefinition
                    {
                        AttributeName = "id",
                        AttributeType = "S"
                    },
                    new AttributeDefinition
                    {
                        AttributeName = "Name",
                        AttributeType = "S"
                    }
                },
                KeySchema = new List<KeySchemaElement>()
                {
                    new KeySchemaElement
                    {
                        AttributeName = "id",
                        KeyType = "HASH"
                    },
                    new KeySchemaElement
                    {
                        AttributeName = "Name",
                        KeyType = "RANGE"
                    }
                },
            };

            createRequest.ProvisionedThroughput = new ProvisionedThroughput(1, 1);

            CreateTableResponse createResponse = null;
            try
            {
                createResponse = client.CreateTable(createRequest);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Error: failed to create the new table; " + ex.Message);
            }

            return createResponse;
        }

        private void ConnectDatabase()
        {
            AWSCredentials credentials = new BasicAWSCredentials("AKIAJSUSXA3UI52JTF7Q", "76N9IecPD7JVuONTfyw8TKhVV1kvI85e+1a1dOSq");
            this.client = new AmazonDynamoDBClient(credentials);
        }

        /// <summary>
        /// Populates the Guest List
        /// </summary>
        public void PopulateGuestList()
        {
            if (client == null)
                ConnectDatabase();

            try
            {
                List<string> tables = client.ListTables().TableNames;
                if (!tables.Contains("TABLE_Guests"))
                {
                    CreateTableResponse response = CreateGuestTable();
                    if (response.TableDescription.TableStatus == TableStatus.ACTIVE)
                    {
                        Populate();
                    }
                }
                else
                {
                    Guests.Clear();
                    Populate();
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Populate()
        {
            guestsTable = Table.LoadTable(client, "TABLE_Guests");
            var scanRequest = new ScanRequest
            {
                TableName = "TABLE_Guests"
            };

            var guests = client.Scan(scanRequest);

            if (guests.Items.Count > 0)
            {
                guests.Items.ForEach(item =>
                {
                    Guests.Add(new Guest
                    {
                        Id = item["id"].S,
                        Name = item["Name"].S,
                        DesignatedSeats = item["DesignatedSeats"].S,
                        HasFan = item["HasFan"].BOOL,
                        CheckInTime = item["CheckInTime"].S,
                        IsCheckedIn = item["IsCheckedIn"].BOOL
                    });
                });

                Guests.OrderBy(item => item.Name);
                OnPropertyChanged("Guests");
            }
        }

        /// <summary>
        /// Uploads the guest list to AWS DB
        /// </summary>
        private void UploadGuestListCommandExecute()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "xls files (*.xls)|*.xls|All files (*.*)|*.*";

            bool? dialogResult = dialog.ShowDialog();

            if (dialogResult.Value)
            {
                string fileName = dialog.FileName;

                OleDbConnection oledbConn = null;
                oledbConn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0; Data Source=" + fileName + "; Extended Properties= \"Excel 8.0;HDR=Yes;IMEX=2\"");
                oledbConn.Open();

                OleDbCommand cmd = new OleDbCommand(); ;
                OleDbDataAdapter oledbAdapater = new OleDbDataAdapter();
                DataSet guests = new DataSet();

                cmd.Connection = oledbConn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM [Worksheet$]";
                oledbAdapater = new OleDbDataAdapter(cmd);
                oledbAdapater.Fill(guests, "Worksheet");

                var table = Table.LoadTable(client, "TABLE_Guests");
                guests.Tables[0].AsEnumerable().ToList().ForEach(guest =>
                {
                    var request = new PutItemRequest
                    {
                        TableName = "TABLE_Guests",
                        Item = new Dictionary<string, AttributeValue>()
                        {
                            {"id", new AttributeValue { S = Guid.NewGuid().ToString() } },
                            { "Name", new AttributeValue { S = guest["Last Name"] + ", " + guest["First Name"] }},
                            { "HasFan", new AttributeValue { BOOL = ConvertStringToBool(guest["HasFan"].ToString()) } },
                            { "DesignatedSeats", new AttributeValue { S = guest["DesignatedSeats"].ToString() } }, //TODO:
                            { "CheckInTime", new AttributeValue { S = " " } },
                            { "IsCheckedIn", new AttributeValue { BOOL = false } }
                        }
                    };

                    client.PutItem(request);
                });
            }
        }

        private bool ConvertStringToBool(string stringValue)
        {
            if (stringValue.ToLower() == "no")
                return false;

            if (stringValue.ToLower() == "yes")
                return true;

            return false;
        }

        private bool CanUploadGuestList()
        {
            return bool.Parse(ConfigurationManager.AppSettings["CanUpload"]);
        }

        private bool CanCheckInGuest()
        {
            return SelectedGuest != null && SelectedGuest.IsCheckedIn == false;
        }

        private void CheckInGuestCommandExecute()
        {
            try
            {
                var table = Table.LoadTable(client, "TABLE_Guests");
                var entry = new Document();
                entry["id"] = SelectedGuest.Id;
                entry["Name"] = SelectedGuest.Name;
                entry["CheckInTime"] = DateTime.Now.ToLocalTime().ToString();
                entry["IsCheckedIn"] = new DynamoDBBool(true);
                table.UpdateItem(entry);

                SelectedGuest.IsCheckedIn = true;
                SelectedGuest.CheckInTime = DateTime.Now.ToLocalTime().ToString();

                CheckInGuestCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("Guests");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
