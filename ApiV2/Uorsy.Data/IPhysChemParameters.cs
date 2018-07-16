namespace Uorsy.Data
{
    public interface IPhysChemParameters
    {
        double Mw { get; }
        double Logp { get; }
        int Hba { get; }
        int Hbd { get; }
        int Rotb { get; }
        double Tpsa { get; }
        double Fsp3 { get; }
        int Hac { get; }
        int RingCount { get; }
    }
}
