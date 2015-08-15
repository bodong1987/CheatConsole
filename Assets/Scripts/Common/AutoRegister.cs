using System;

namespace Assets.Scripts.Common
{
    public interface IIdentifierAttribute<TIdentifier>
    {
        TIdentifier ID { get; }
    }

    public class AutoRegisterAttribute : Attribute
    {

    }
}