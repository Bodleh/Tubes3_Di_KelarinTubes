<h1 align="center">Tugas Besar 3 IF2211 Strategi Algoritma</h1>
<h1 align="center">Semester II tahun 2023/2024</h1>
<h1 align="center">Pemanfaatan Pattern Matching dalam Membangun Sistem Deteksi Individu Berbasis Biometrik Melalui Citra Sidik Jari</h1>

<h1 align=""> Kelompok 40: Di_KelarinTubes </h1>

| NIM      | Nama               | Kelas |
| -------- | ------------------ | ----- |
| 13522047 | Farel Winalda      | K-01  |
| 13522111 | Ivan Hendrawan Tan | K-01  |

## Table of Contents

- [Deskripsi Program](#deskripsi-program)
- [Requirements Program](#requirements-program)
- [Setup](#set-up-program)
- [Link](#link)

## Deskripsi Program

Di era digital ini, keamanan data dan akses menjadi semakin penting. Perkembangan teknologi membuka peluang untuk berbagai metode identifikasi yang canggih dan praktis. Beberapa metode umum yang sering digunakan seperti kata sandi atau pin, namun memiliki kelemahan seperti mudah terlupakan atau dicuri. Oleh karena itu, biometrik menjadi alternatif metode akses keamanan yang semakin populer. Salah satu teknologi biometrik yang banyak digunakan adalah identifikasi sidik jari. Sidik jari setiap orang memiliki pola yang unik dan tidak dapat ditiru, sehingga cocok untuk digunakan sebagai identitas individu.
Pattern matching merupakan teknik penting dalam sistem identifikasi sidik jari. Teknik ini digunakan untuk mencocokkan pola sidik jari yang ditangkap dengan pola sidik jari yang terdaftar di database. Algoritma pattern matching yang umum digunakan adalah Bozorth dan Boyer-Moore. Algoritma ini memungkinkan sistem untuk mengenali sidik jari dengan cepat dan akurat, bahkan jika sidik jari yang ditangkap tidak sempurna.
Dengan menggabungkan teknologi identifikasi sidik jari dan pattern matching, dimungkinkan untuk membangun sistem identifikasi biometrik yang aman, handal, dan mudah digunakan. Sistem ini dapat diaplikasikan di berbagai bidang, seperti kontrol akses, absensi karyawan, dan verifikasi identitas dalam transaksi keuangan.


## Requirements Program

1. .NET Version 8
2. Avalonia UI

## Set Up Program

1.  Jalankan perintah berikut pada terminal
    ```bash
    git clone https://github.com/Bodleh/Tubes3_Di_KelarinTubes.git
    ```
2.  Masukkan juga perintah berikut
    ```bash
    cd Tubes3_Di_KelarinTubes
    ```
3.  Setelah berada pada root directory, buatlah sebuah terminal baru pada direktori yang sama
4.  Dari terminal pertama, jalankan perintah-perintah berikut
    ```bash
    cd src/client
    dotnet build
    dotnet run
    ```
5. Dari terminal kedua, jalankan perintah berikut
    ```bash
    cd src/server
    dotnet build
    dotnet run
    ```
6. Setelah itu, buat sebuah folder bernama "test" di root dan didalamnya buat lagi sebuah folder bernama "test"
7. Kemudian unduh dataset sidik jari pada link berikut https://drive.google.com/drive/folders/1KvJcSkQXgjz4lb1Ayg71KQB5zrFIPNOG?usp=sharing
8.  Selamat menggunakan :)

## Link

- Repository : https://github.com/Bodleh/Tubes3_Di_KelarinTubes.git