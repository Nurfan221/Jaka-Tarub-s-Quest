public interface IUniqueIdentifiable
{
    // Setiap objek yang memakai interface ini WAJIB punya properti UniqueID
    string UniqueID { get; set; }

    // Mereka juga WAJIB menyediakan informasi ini untuk generator ID
    string GetBaseName(); // Misal: "PohonApelBesar", "BatuTembagaKecil"
    string GetObjectType(); // Misal: "PohonApel", "BatuTembaga"
    EnvironmentHardnessLevel GetHardness();
    string GetVariantName();
}