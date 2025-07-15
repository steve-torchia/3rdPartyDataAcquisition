using System.ComponentModel;

namespace DP.Base.Contracts.ComponentModel
{
    public interface IPropertyDescriptorProvider
    {
        PropertyDescriptor[] GetProperties();
    }
}
