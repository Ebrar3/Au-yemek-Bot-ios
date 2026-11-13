import 'package:flutter/material.dart';
import '../services/auth_service.dart';

class AdminPanelScreen extends StatelessWidget {
  const AdminPanelScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return ListenableBuilder(
      listenable: AuthService.instance,
      builder: (context, _) {
        final auth = AuthService.instance;

        if (!auth.girisYapildiMi || !auth.yoneticiMi) {
          return Center(
            child: Padding(
              padding: const EdgeInsets.all(24),
              child: Card(
                color: Theme.of(context).colorScheme.errorContainer,
                child: Padding(
                  padding: const EdgeInsets.all(24),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      const Icon(Icons.lock_person, size: 48),
                      const SizedBox(height: 12),
                      Text(
                        'Yönetici Yetkisi Gerekli',
                        style: Theme.of(context).textTheme.titleLarge,
                      ),
                      const SizedBox(height: 8),
                      Text(
                        auth.girisYapildiMi
                            ? 'Aktif hesabınız (${auth.aktifKullanici!.rol}) bu ekrana erişemez. Yönetim ekranları yalnızca "Yönetici" rolüne açıktır.'
                            : 'Yönetim ekranlarına erişmek için lütfen Giriş sekmesinden "Yönetici" hesabıyla oturum açınız.',
                        textAlign: TextAlign.center,
                      ),
                    ],
                  ),
                ),
              ),
            ),
          );
        }

        return ListView(
          padding: const EdgeInsets.all(16),
          children: [
            Card(
              color: Colors.amber.shade50,
              child: ListTile(
                leading: const Icon(Icons.verified_user, color: Colors.deepOrange),
                title: Text('Aktif Yönetici: ${auth.aktifKullanici!.adSoyad}'),
                subtitle: Text('${auth.aktifKullanici!.eposta} • Rol: ${auth.aktifKullanici!.rol}'),
              ),
            ),
            const SizedBox(height: 12),
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
      },
    );
  }
}
