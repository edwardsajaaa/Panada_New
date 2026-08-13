# Panada Project

**Panada Project** adalah sebuah game berbasis cerita yang mengangkat tema sosial tentang urbanisasi dan fenomena perpindahan pemuda desa ke kota besar. 

## Sinopsis Cerita
Permainan dibuka dengan sebuah sorotan berita utama: *"Gelombang Migrasi Pemuda Desa Meningkat: Pemuda Desa Ramai-Ramai Tinggalkan Kampung Halaman untuk Mengejar Harapan di Kota"*. 

Pemain akan dibawa ke dalam kisah karakter-karakter (salah satunya bernama Nathan) yang harus menghadapi realita kehidupan, konflik batin antara mengejar impian di kerasnya ibu kota, dan rasa rindu akan kampung halaman yang mulai sepi ditinggalkan penerusnya.

## Fitur Sistem (Tahap Awal)
- **Sistem Dialog Dinamis**: Dilengkapi dengan UI dialog yang mendukung banyak karakter, serta dilengkapi dengan efek transisi teks (Fade In & Fade Out) agar perpindahan obrolan terasa halus dan tidak kaku.
- **Intro Sinematik**: Urutan adegan pengenalan (Intro Sequence) yang diatur menggunakan Coroutine, mencakup transisi *Fade from Black*, animasi *Booting TV*, hingga transisi masuk ke dalam siaran berita.
- **UI Responsif**: Mendukung **TextMeshPro** untuk rendering teks font bergaya pixel art yang tajam.

## Fitur Sistem (Update Terbaru)
- **Sistem Dialog Kamar (SistemDialogKamar.cs)**: Sistem dialog yang lebih modular khusus untuk scene kamar. Mendukung kustomisasi penuh melalui Inspector untuk transisi masuk (Pop-Up) dan transisi keluar (Fade, Pop-Out, atau langsung hilang) tanpa memerlukan hardcode.
- **Interaksi Handphone & Panel Pesan (BukaPanelPesan.cs & ResponPilihanPesan.cs)**: Mekanisme membaca pesan dengan efek sinematik (animasi kelopak mata berkedip menggunakan shader/material) yang kemudian memunculkan HP di layar. Dilengkapi dengan sistem pilihan (*Balas Sekarang* / *Nanti Saja*) yang saling terhubung secara mulus dengan sistem dialog.
- **Sistem Animasi UI Universal (AnimasiTombolMenu.cs)**: Script animasi *all-in-one* berbasis kode murni (tanpa Unity Animator). Mendukung efek *breathing/floating* (mengambang), efek perbesaran saat *hover*, serta berbagai mode transisi masuk/keluar (*PopIn*, *Slide*, *Fade*) yang bisa dikonfigurasi langsung lewat Inspector dan berjalan secara dinamis.
- **Sistem Pergerakan 2.5D (PlayerMovement25D.cs)**: Sistem kontrol karakter yang *Camera-Relative* (bergerak menyesuaikan sudut pandang kamera, sehingga tombol Kiri/Kanan selalu akurat di layar terlepas dari rotasi ruangan). Sudah terintegrasi langsung dengan Unity Animator untuk memicu animasi berjalan.
- **Custom Shader 2.5D (Sprite25D.shader)**: Shader *Alpha Cutout* buatan khusus untuk Sprite 2D di dunia 3D (URP). Shader ini mengabaikan arah kemiringan cahaya (Normal) dan hanya menggunakan jarak (*Distance Attenuation*) sehingga karakter tidak akan pernah menjadi siluet hitam pekat saat berada di sudut ruangan.
- **Pencahayaan & Visual Sinematik Kamar**: Menerapkan standar pencahayaan game *Indie Profesional* menggunakan perpaduan material *Emission*, *Point Light*, dan URP Post-Processing (*Bloom* & *ACES Tonemapping*) untuk menghasilkan ruangan yang hangat dan dramatis.
- **Sistem Transisi Main Menu & Sub-Panel (TransisiMenuUI.cs)**: Logika navigasi UI yang mulus dengan efek transisi antar panel. Dilengkapi fitur *Smart Back Button* dan hierarki bersarang (seperti *Setting Panel* -> *Audio Panel*) yang otomatis menjaga sisa objek (seperti dekorasi meja) tetap utuh.
- **Visualisasi Volume Audio & Save Data (PengaturanAudioUI.cs)**: Sistem interaktif untuk pengaturan volume (BGM, Main, Voice, SFX) dengan indikator balok (0-5) yang menyala secara dinamis lewat penggantian Sprite (*Sprite Swapping*). Didukung sistem *PlayerPrefs* otomatis agar pengaturan pemain tidak riset saat *game* ditutup.
- **Sistem Loading Screen Asinkron (LoadingScreenController.cs)**: Memuat *scene* secara mulus di latar belakang (*AsyncOperation*) dengan ditemani panel layar pudar (*Fade Out / Fade In*) agar tidak terjadi *freeze* saat perpindahan tempat.
- **Sistem Background Music Global (BGMManager.cs & BGMTrigger.cs)**: Sistem audio BGM persisten (DontDestroyOnLoad) yang mendukung transisi mulus (*Crossfade*) antar scene. Dilengkapi pengaturan volume khusus per lagu dan fitur *Fade-to-Silence* ("Jadikan Hening") saat panel tertentu dibuka.
- **Animasi Slideshow Otomatis (AnimasiSlideshow.cs)**: Komponen yang secara otomatis mendeteksi objek *Image* anak dan memutarnya bergantian seperti *slideshow*. Dilengkapi deteksi cerdas yang akan mengabaikan objek *Text* agar tulisan tidak ikut berkedip.
- **Sistem Anti-Spam Klik (BukaPanelPesan.cs)**: Implementasi *flagging* keamanan (`sedangDiproses`) pada tombol-tombol interaktif guna mencegah eksekusi beruntun ganda (*spam click*) yang dapat memicu error *coroutine* atau tumpang tindih animasi.
- **Pembersihan Kode Massal (Clean Code Refactoring)**: Seluruh 38 script C# pada proyek telah dibersihkan secara massal menggunakan metode heuristik; menghilangkan ratusan baris komentar receh/XML usang/emoticon untuk mempertahankan standar kualitas kode yang profesional, ringan, dan rapi.
## Setup Project
Game ini dikembangkan menggunakan **Unity Engine**.
1. *Clone/Download* repositori ini.
2. Buka proyek melalui **Unity Hub**.
3. Buka folder `Assets/Scenes` atau langsung cek fungsionalitas di `Assets/Script/`.
4. Tekan tombol Play untuk melihat transisi dan mencoba dialognya secara langsung.

---
*Proyek ini masih dalam tahap pengembangan aktif oleh Developer Indie yang penuh semangat!*
