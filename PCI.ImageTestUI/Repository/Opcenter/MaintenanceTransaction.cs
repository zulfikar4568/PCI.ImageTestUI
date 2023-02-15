using Camstar.WCF.ObjectStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCI.ImageTestUI.Repository.Opcenter
{
    public class MaintenanceTransaction
    {
        private readonly Driver.Opcenter.Helper _helper;

        private readonly Driver.Opcenter.MaintenanceTransaction _maintenanceTxn;
        public MaintenanceTransaction(Driver.Opcenter.Helper helper, Driver.Opcenter.MaintenanceTransaction maintenanceTxn)
        {
            _helper = helper;
            _maintenanceTxn = maintenanceTxn;
        }
        public UserDataCollectionDefChanges GetUserDataCollectionDef(string UserDataCollectionDefName, string UserDataCollectionDefRevision = "", bool IgnoreException = true)
        {
            RevisionedObjectRef objectToChange = new RevisionedObjectRef(UserDataCollectionDefName);
            if (UserDataCollectionDefName != "" && UserDataCollectionDefRevision != "")
            {
                objectToChange = new RevisionedObjectRef(UserDataCollectionDefName, UserDataCollectionDefRevision);
            }
            UserDataCollectionDefChanges_Info userDataCollectionDefInfo = new UserDataCollectionDefChanges_Info();

            userDataCollectionDefInfo.Name = new Info(true);
            userDataCollectionDefInfo.Revision = new Info(true);
            userDataCollectionDefInfo.Description = new Info(true);
            userDataCollectionDefInfo.DataPoints = new DataPointChanges_Info()
            {
                RequestValue = true,
                RequestSelectionValues = true,
                Name = new Info(true),
                ColumnPosition = new Info(true),
                DataType = new Info(true),
                Description1 = new Info(true),
                DecimalScale = new Info(true),
                DisplayName = new Info(true),
                RowPosition = new Info(true),
                TxnDate = new Info(true),
                TypeName = new Info(true),
                ParentName = new Info(true),
                Parent = new Info(true),
            };

            return _maintenanceTxn.UserDataCollectionInfo(objectToChange, userDataCollectionDefInfo, IgnoreException);
        }

        public bool SaveDocument(string Name, string Revision, string Identifier, bool UploadFile, string IsRevOfRcd = "", string Description = "", string Notes = "", bool IgnoreException = true)
        {
            DocumentChanges objectChanges = new DocumentChanges();
            objectChanges.Name = new Primitive<string> { Value = Name };
            objectChanges.Revision = new Primitive<string> { Value = Revision };
            if (UploadFile)
            {
                //if (!System.IO.File.Exists(Identifier)) throw new Exception($"{Identifier} does not exist");
                objectChanges.FileLocation = new Primitive<string> { Value = System.IO.Path.GetDirectoryName(Identifier) };
                objectChanges.FileName = new Primitive<string> { Value = System.IO.Path.GetFileName(Identifier) };
                objectChanges.Identifier = new Primitive<string> { Value = System.IO.Path.GetFileName(Identifier) };
                objectChanges.UploadFile = new Primitive<bool> { Value = true };
            }
            else
            {
                objectChanges.BrowseMode = BrowseModeEnum.Url;
                objectChanges.Identifier = new Primitive<string> { Value = Identifier };
            }
            if (IsRevOfRcd != "") objectChanges.IsRevOfRcd = new Primitive<bool> { Value = Convert.ToBoolean(IsRevOfRcd) };
            if (Description != "") objectChanges.Description = new Primitive<string> { Value = Description };
            if (Notes != "") objectChanges.Notes = new Primitive<string> { Value = Notes };

            return _maintenanceTxn.SaveDocument(objectChanges, IgnoreException);
        }
    }
}
