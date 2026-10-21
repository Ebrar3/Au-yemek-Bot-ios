import 'package:flutter/material.dart';

class PopularFoodsScreen extends StatelessWidget {
  const PopularFoodsScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.all(16),
      children: [
        Text('En Beğenilen Yemekler', style: Theme.of(context).textTheme.titleLarge),
        const SizedBox(height: 8),
        const Card(
          child: ListTile(
            leading: Icon(Icons.emoji_events, color: Colors.amber),
            title: Text('Mercimek Çorbası'),
            subtitle: Text('Çorba • 180 kcal'),
            trailing: Text('4.6 / 5.0'),
          ),
        ),
        const Card(
          child: ListTile(
            leading: Icon(Icons.thumb_up_alt_outlined, color: Colors.green),
            title: Text('Orman Kebabı'),
            subtitle: Text('Ana Yemek • 420 kcal'),
            trailing: Text('4.4 / 5.0'),
          ),
        ),
        const SizedBox(height: 16),
        Text('En Az Beğenilen Yemekler', style: Theme.of(context).textTheme.titleLarge),
        const SizedBox(height: 8),
        const Card(
          child: ListTile(
            leading: Icon(Icons.trending_down, color: Colors.redAccent),
            title: Text('Kereviz Yemeği'),
            subtitle: Text('Ana Yemek • 240 kcal'),
            trailing: Text('2.3 / 5.0'),
          ),
        ),
      ],
    );
  }
}
