import 'package:flutter/material.dart';
import '../models/yemek.dart';

class FoodDetailRatingScreen extends StatefulWidget {
  final Yemek yemek;

  const FoodDetailRatingScreen({super.key, required this.yemek});

  @override
  State<FoodDetailRatingScreen> createState() => _FoodDetailRatingScreenState();
}

class _FoodDetailRatingScreenState extends State<FoodDetailRatingScreen> {
  final _formKey = GlobalKey<FormState>();
  int _secilenPuan = 5;
  final _yorumController = TextEditingController();

  @override
  void dispose() {
    _yorumController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(widget.yemek.ad)),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Form(
          key: _formKey,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Text(
                '${widget.yemek.kategoriAdi} • ${widget.yemek.kalori ?? 0} kcal',
                style: Theme.of(context).textTheme.titleMedium,
              ),
              const SizedBox(height: 16),
              DropdownButtonFormField<int>(
                initialValue: _secilenPuan,
                decoration: const InputDecoration(
                  labelText: 'Puanınız (1–5)',
                  border: OutlineInputBorder(),
                ),
                items: [1, 2, 3, 4, 5]
                    .map((p) => DropdownMenuItem(value: p, child: Text('$p Yıldız')))
                    .toList(),
                onChanged: (val) => setState(() => _secilenPuan = val ?? 5),
              ),
              const SizedBox(height: 16),
              TextFormField(
                controller: _yorumController,
                maxLines: 3,
                decoration: const InputDecoration(
                  labelText: 'Yorumunuz',
                  border: OutlineInputBorder(),
                ),
                validator: (val) {
                  if (val != null && val.length > 500) {
                    return 'Yorum en fazla 500 karakter olabilir.';
                  }
                  return null;
                },
              ),
              const SizedBox(height: 16),
              FilledButton(
                onPressed: () {
                  if (_formKey.currentState!.validate()) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(content: Text('Değerlendirmeniz kaydedildi.')),
                    );
                    Navigator.of(context).pop();
                  }
                },
                child: const Text('Değerlendirmeyi Gönder'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
