using SearchV2.Abstractions;

namespace Uorsy.Data
{
    public interface IMoleculeData : IPhysChemParameters, IWithReference<string>
    {
        string Id { get; }
        string Smiles { get; }
        string Name { get; }
        string InChIKey { get; }
        string Cas { get; }
        string Mfcd { get; }
        int PriceCategoryId { get; }
    }
}
