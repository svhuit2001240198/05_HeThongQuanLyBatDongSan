"""Kiểm tra dữ liệu mẫu offline, không cần MongoDB hay thư viện ngoài."""
from pathlib import Path
import base64,collections,hashlib,hmac,json,re,struct

root=Path(__file__).resolve().parents[1]/'DoAn_NoSQL_Nhom5/Data/mock'
users=[json.loads(x) for x in (root/'Users.jsonl').read_text(encoding='utf-8').splitlines() if x.strip()]
bds=[json.loads(x) for x in (root/'BDS-demo.jsonl').read_text(encoding='utf-8').splitlines() if x.strip()]
codes={u['MaUser'] for u in users}; emails={u['Email'] for u in users}; props={p['MaBDS'] for p in bds}
assert 10000<=len(users)<=50000
assert len(codes)==len(users)==len(emails), 'MaUser/Email bị trùng'
assert len(props)==len(bds), 'MaBDS bị trùng'
assert all('_id' not in u for u in users), '_id nên để MongoDB tự tạo khi import'
for u in users:
    assert u['VaiTro'] in ['nguoi_mua','nguoi_ban','moi_gioi','admin']
    assert ('ThongTinMoiGioi' in u)==(u['VaiTro']=='moi_gioi'), 'Cấu trúc môi giới sai vai trò'
    assert re.fullmatch(r'0\d{9,10}',u['SDT'])
    assert u['Email']==u['Email'].strip().lower()
    assert len(set(u['BDSDaLuu']))==len(u['BDSDaLuu'])
    assert set(u['BDSDaLuu'])<=props, 'BDSDaLuu tham chiếu mã không tồn tại'
    seen=set()
    for r in u['DanhGia']:
        assert u['VaiTro']=='moi_gioi' and r['MaUserDanhGia'] in codes and r['MaUserDanhGia']!=u['MaUser']
        assert r['MaUserDanhGia'] not in seen; seen.add(r['MaUserDanhGia'])
        assert 1<=r['SoSao']<=5
        assert not r.get('MaBDS') or r['MaBDS'] in props
    assert len({n['MaThongBao'] for n in u['ThongBao']})==len(u['ThongBao'])
    for n in u['ThongBao']:assert not n.get('MaBDS') or n['MaBDS'] in props
for p in bds:
    assert p['MaChuSoHuu'] in codes and p['MaMoiGioi'] in codes
    assert next(u for u in users if u['MaUser']==p['MaMoiGioi'])['VaiTro']=='moi_gioi'
    assert '$numberDecimal' in p['Gia'] and int(p['Gia']['$numberDecimal'])>0
    assert p['LoaiBDS'] in ['can_ho','dat_nen','biet_thu']
    assert p['ThuocTinhRieng']
# Kiểm tra các hash khác nhau; dữ liệu giả định dùng chung một hash cho các user sinh ngẫu nhiên.
for value in set(u['MatKhau'] for u in users):
    raw=base64.b64decode(value);assert raw[0]==1
    prf,iterations,saltlen=struct.unpack('>III',raw[1:13]);assert prf==2 and iterations>=100000 and saltlen>=16
    salt=raw[13:13+saltlen];key=raw[13+saltlen:]
    assert hmac.compare_digest(key,hashlib.pbkdf2_hmac('sha512',b'Demo@12345',salt,iterations,len(key)))
assert len({tuple(sorted(u.keys())) for u in users})>=2
assert len({tuple(sorted(p['ThuocTinhRieng'].keys())) for p in bds})==3
print('PASS: khối lượng, mã duy nhất, tham chiếu, vai trò, nested data, số sao và định dạng hash Identity V3.')
print(json.dumps({'Users':len(users),'BDS_demo':len(bds),'VaiTro':dict(collections.Counter(u['VaiTro'] for u in users))},ensure_ascii=False))
