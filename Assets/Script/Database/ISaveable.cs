public interface ISaveable
{

    // Fungsi untuk "mengemas" datanya menjadi sebuah objek yang bisa disimpan.
    object CaptureState();

    //  Fungsi untuk "membongkar" data dan mengembalikannya ke keadaan semula.
    void RestoreState(object state);
}