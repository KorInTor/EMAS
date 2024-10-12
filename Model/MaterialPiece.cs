using CommunityToolkit.Mvvm.ComponentModel;
using Model.Enum;

namespace Model
{
    public class MaterialPiece : ObservableObject, IEquatable<MaterialPiece>, IStorableObject
    {
        private int id;

        private string type;

        private string name;

        private string units;

        private int amount;

        private string? extras;

        private string inventoryNumber;

        private string storageType;

        private string description;

        private string? comment;



        public int Id
        {
            get => id;
            set => SetProperty(ref id, value);
        }

        public string Type
        {
            get => type;
            set => SetProperty(ref type, value);
        }

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }
        public string Units
        {
            get => units;
            set => SetProperty(ref units, value);
        }
        public int Amount
        {
            get => amount;
            set => SetProperty(ref amount, value);
        }

        public string? Extras
        {
            get => extras;
            set => SetProperty(ref extras, value);
        }

        public string InventoryNumber
        {
            get => inventoryNumber;
            set => SetProperty(ref inventoryNumber, value);
        }
        public string StorageType
        {
            get => storageType;
            set => SetProperty(ref storageType, value);
        }
        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }
        public string Comment
        {
            get => comment;
            set => SetProperty(ref comment, value);
        }

        public StorableObjectType StorableObjectType => StorableObjectType.Material;

        public bool Equals(MaterialPiece? other)
        {
            if (other == null || GetType() != other.GetType())
            {
                return false;
            }

            return Type == other.Type &&
                   Units == other.Units &&
                   Amount == other.Amount &&
                   Extras == other.Extras &&
                   StorageType == other.StorageType &&
                   Comment == other.Comment;
        }
    }
}
