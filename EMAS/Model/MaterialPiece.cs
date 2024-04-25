using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model
{
    internal class MaterialPiece :ObservableObject, IEquatable<MaterialPiece>, IStorableObject, ILocationBounded
    {
        private string type;

        private string units;

        private int amount;

        private string? extras;

        private string storageType;

        private string comment;



        public int Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int LocationId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Type
        {
            get => type;
            set => SetProperty(ref type, value);
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
        public string StorageType 
        {
            get => storageType;
            set => SetProperty(ref storageType, value);
        }

        public string Comment 
        {
            get => comment;
            set => SetProperty(ref comment, value);
        }

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
