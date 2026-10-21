import 'package:flutter/material.dart';

class AdminPanelScreen extends StatelessWidget {
  const AdminPanelScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Text(
          'Yönetici Paneli (Yalnızca Yönetici Rolü)',
          style: Theme.of(context).textTheme.titleLarge,
        ),
        const SizedBox(height: 12),
        const Card(
          child: ListTile(
            leading: Icon(Icons.restaurant_menu),
            title: Text('Menü Yönetimi (CRUD)'),
            subtitle: Text('Günlük öğle ve akşam menülerini ekle, düzenle veya sil'),
            trailing: Icon(Icons.chevron_right),
          ),
        ),
        const Card(
          child: ListTile(
            leading: Icon(Icons.fastfood),
            title: Text('Yemek ve Kategori Yönetimi (CRUD)'),
            subtitle: Text('Yemekleri, kategorileri ve kalori değerlerini yönet'),
            trailing: Icon(Icons.chevron_right),
          ),
        ),
        const Card(
          child: ListTile(
            leading: Icon(Icons.upload_file),
            title: Text('Aylık Excel Menü Yükleme'),
            subtitle: Text('Üniversitenin öğle/akşam Excel menü dosyalarını içe aktar'),
            trailing: Icon(Icons.chevron_right),
          ),
        ),
        const Card(
          child: ListTile(
            leading: Icon(Icons.rate_review),
            title: Text('Öğrenci Yorum Moderasyonu'),
            subtitle: Text('Yapılan yorumları onayla veya sil'),
            trailing: Icon(Icons.chevron_right),
          ),
        ),
      ],
    );
  }
}
