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
- **Deteksi UI Debugging (CekKlikUI.cs)**: Alat bantu *debugging* sederhana untuk memastikan jangkauan *Raycast* dan responsivitas klik pada elemen UI.

## Setup Project
Game ini dikembangkan menggunakan **Unity Engine**.
1. *Clone/Download* repositori ini.
2. Buka proyek melalui **Unity Hub**.
3. Buka folder `Assets/Scenes` atau langsung cek fungsionalitas di `Assets/Script/`.
4. Tekan tombol Play untuk melihat transisi dan mencoba dialognya secara langsung.

---
*Proyek ini masih dalam tahap pengembangan aktif oleh Developer Indie yang penuh semangat!*
