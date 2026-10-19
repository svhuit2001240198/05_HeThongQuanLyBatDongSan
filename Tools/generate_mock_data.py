"""Sinh dữ liệu offline bằng Python chuẩn. Ứng dụng chính và lệnh sinh tương đương dùng C#.
Chạy: python Tools/generate_mock_data.py --count 10000
"""
from pathlib import Path
import argparse, base64, collections, csv, hashlib, json, random, secrets, struct
from datetime import datetime, timedelta, timezone

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'DoAn_NoSQL_Nhom5/Data/mock'
BASE=datetime(2026,10,10,12,tzinfo=timezone.utc)
PASSWORD='Demo@12345'
def date(dt): return {'$date':dt.isoformat(timespec='milliseconds').replace('+00:00','Z')}
def money(n): return {'$numberDecimal':str(n)}
def password_hash():
    # Định dạng ASP.NET Core Identity V3: marker, PRF, iterations, saltLength, salt, subkey.
    salt=secrets.token_bytes(16)
    key=hashlib.pbkdf2_hmac('sha512',PASSWORD.encode(),salt,100000,32)
    return base64.b64encode(b'\x01'+struct.pack('>III',2,100000,16)+salt+key).decode()

def generate(count):
    rng=random.Random(20261010); OUT.mkdir(parents=True,exist_ok=True)
    areas=['TP. Hồ Chí Minh','Hà Nội','Đà Nẵng','Bình Dương','Đồng Nai']
    family=['Nguyễn','Trần','Lê','Phạm','Hoàng','Võ','Đặng','Bùi']
    given=['Minh Anh','Hoàng Nam','Quốc Huy','Ngọc Linh','Thanh Trúc','Gia Bảo','Khánh Vy','Đức Phúc','Tuấn Kiệt','Hải Yến']
    fixtures=[('ADMIN','Quản trị HomeLand','admin','admin'),('BUYER','Nguyễn Công Danh','buyer','nguoi_mua'),('SELLER','Phạm Quang Vinh','seller','nguoi_ban'),('BROKER','Trần Minh Anh','broker','moi_gioi'),('BROKER2','Lê Hoàng Nam','broker2','moi_gioi')]
    users=[]; shared_mock_hash=password_hash()
    for i in range(count):
        if i<5:
            code,name,email,role=fixtures[i]; code='USR-DEMO-'+code; email+='@homeland.test'; hashed=password_hash(); created=BASE-timedelta(days=30)
        else:
            code=f'USR-{i:05d}'; name=rng.choice(family)+' '+rng.choice(given); email=f'user{i:05d}@example.test'; hashed=shared_mock_hash
            role='nguoi_mua' if i%10<6 else 'nguoi_ban' if i%10<8 else 'moi_gioi'; created=BASE-timedelta(days=rng.randrange(1,730))
        words=name.split()
        u={'MaUser':code,'Ho':words[0],'TenDem':' '.join(words[1:-1]),'Ten':words[-1],'HoTen':name,'GioiTinh':'nam' if i%2==0 else 'nu','Email':email,'SDT':f'09{10000000+i:08d}',
           'MatKhau':hashed,'VaiTro':role,'NgayTao':date(created),'TrangThaiTaiKhoan':'hoat_dong','SecurityStamp':secrets.token_hex(16),'DanhGia':[],'BDSDaLuu':[],'ThongBao':[]}
        if role=='moi_gioi':
            u['ThongTinMoiGioi']={'AnhDaiDien':'','GioiThieu':'Tư vấn căn hộ và nhà ở. Đồng hành cùng khách hàng từ tìm kiếm đến xem nhà.','KinhNghiem':8 if i<5 else rng.randrange(1,16),
              'KhuVucHoatDong':['TP. Hồ Chí Minh','Bình Dương'] if i<5 else rng.sample(areas,rng.randrange(1,4)), 'TrangThai':'ngung_hoat_dong' if i>=5 and i%17==0 else 'dang_hoat_dong'}
        users.append(u)
    buyers=[u for u in users if u['VaiTro']=='nguoi_mua' and not u['MaUser'].startswith('USR-DEMO')]
    for i,u in enumerate(users):
        u['BDSDaLuu']=[f'BDS-DEMO-{n:03d}' for n in rng.sample(range(1,19),rng.randrange(0,7))]
        for j in range(rng.randrange(0,4)):
            n={'MaThongBao':f'TB-{u["MaUser"]}-{j}','LoaiThongBao':'he_thong','TieuDe':'Hoàn thiện thông tin tài khoản' if j==0 else 'Khám phá những tin đăng mới',
               'NoiDung':'Cập nhật hồ sơ để việc kết nối thuận tiện hơn.' if j==0 else 'Xem và lưu những bất động sản phù hợp với nhu cầu của bạn.','DaDoc':j%2==1,'NgayTao':date(BASE-timedelta(days=j))}
            if j: n['MaBDS']=f'BDS-DEMO-{j:03d}'
            u['ThongBao'].append(n)
        if u['VaiTro']=='moi_gioi':
            for r in rng.sample(buyers,rng.randrange(0,6)):
                u['DanhGia'].append({'MaUserDanhGia':r['MaUser'],'SoSao':rng.randrange(3,6),'NoiDung':'Tư vấn rõ ràng, hỗ trợ nhiệt tình và phản hồi đúng hẹn.','NgayDanhGia':date(BASE-timedelta(days=rng.randrange(1,60)))})
    users[1]['BDSDaLuu']=['BDS-DEMO-001','BDS-DEMO-003','BDS-DEMO-005','BDS-DEMO-007']
    users[1]['ThongBao']=[{'MaThongBao':'TB-DEMO-BUYER-1','LoaiThongBao':'he_thong','TieuDe':'Chào mừng bạn đến HomeLand','NoiDung':'Những lựa chọn cho tổ ấm của bạn bắt đầu từ đây.','DaDoc':False,'NgayTao':date(BASE)},
       {'MaThongBao':'TB-DEMO-BUYER-2','LoaiThongBao':'tin_dang','TieuDe':'Khám phá căn hộ tại Thảo Điền','NoiDung':'Một lựa chọn phù hợp để bạn tham khảo và lưu lại.','MaBDS':'BDS-DEMO-001','DaDoc':False,'NgayTao':date(BASE-timedelta(hours=3))},
       {'MaThongBao':'TB-DEMO-BUYER-3','LoaiThongBao':'he_thong','TieuDe':'Hồ sơ của bạn đã sẵn sàng','NoiDung':'Bạn có thể cập nhật thông tin liên hệ trong mục Hồ sơ cá nhân.','DaDoc':True,'NgayTao':date(BASE-timedelta(days=1))}]
    titles=['Căn hộ ánh sáng tại Thảo Điền','Biệt thự sân vườn ven sông','Đất nền khu dân cư An Phú','Căn hộ ban công nhìn thành phố','Biệt thự hiện đại tại Bình Dương','Đất nền gần công viên','Căn hộ hai phòng ngủ tại Phú Mỹ Hưng','Biệt thự không gian mở','Đất nền mặt tiền Đồng Nai']
    bds=[]
    for i in range(1,19):
        t=(i-1)%3; city='Bình Dương' if i%4==0 else 'Đồng Nai' if i%5==0 else 'TP. Hồ Chí Minh'
        attrs={'SoPhongNgu':2,'SoPhongTam':2,'Tang':i+3} if t==0 else {'SoPhongNgu':4,'SoPhongTam':3,'SoTang':2,'CoHoBoi':i%2==0,'CoSanVuon':True,'ChoDoOto':True} if t==1 else {'ChieuDai':20,'ChieuRong':8,'LoaiDat':'Đất ở','PhapLy':'Sổ hồng'}
        price=3200000000+i*100000000 if t==0 else 12000000000+i*200000000 if t==1 else 2400000000+i*100000000
        bds.append({'MaBDS':f'BDS-DEMO-{i:03d}','MaChuSoHuu':'USR-DEMO-SELLER','MaMoiGioi':'USR-DEMO-BROKER' if i%2 else 'USR-DEMO-BROKER2','LoaiBDS':['can_ho','biet_thu','dat_nen'][t], 'TieuDe':titles[(i-1)%9],
          'DiaChi':{'SoNha':str(12+i),'Duong':f'Đường số {i}','Phuong':'An Phú','Quan':'Thủ Đức','ThanhPho':city},'ViTri':{'KinhDo':106.72+i*.001,'ViDo':10.79+i*.001}, 'DienTich':65+i*2 if t==0 else 150+i*5,'Gia':money(price),'Huong':'Đông Nam',
          'MoTa':'Không gian thoáng, thuận tiện di chuyển và gần các tiện ích sinh hoạt. Liên hệ chủ sở hữu hoặc môi giới để tìm hiểu thông tin và sắp xếp xem thực tế.',
          'HinhAnh':[{'URL':'/img/property-'+['apartment','villa','land'][t]+'.svg','MoTa':'Hình minh họa bất động sản mẫu','ThuTu':1}],'Video':[],'TienIch':['Bãi đỗ xe','Công viên','An ninh 24/7'],'ThuocTinhRieng':attrs,
          'ThongTinGiaoDich':{'LoaiGiaoDich':'ban','TrangThai':'dang_ban','NgayDang':date(BASE-timedelta(days=i)),'NgayHetHan':date(BASE+timedelta(days=90))},'LichSuGia':[]})
    for filename,records in [('Users.jsonl',users),('BDS-demo.jsonl',bds)]:
        with (OUT/filename).open('w',encoding='utf-8') as f:
            for record in records: f.write(json.dumps(record,ensure_ascii=False,separators=(',',':'))+'\n')
    with (OUT/'MaUser_VaiTro.csv').open('w',encoding='utf-8-sig',newline='') as f:
        writer=csv.writer(f);writer.writerow(['MaUser','VaiTro','HoTen']);writer.writerows((u['MaUser'],u['VaiTro'],u['HoTen']) for u in users)
    summary={'Users':len(users),'BDS_demo':len(bds),'VaiTro':dict(collections.Counter(u['VaiTro'] for u in users)),'UsersCoHoSoMoiGioi':sum('ThongTinMoiGioi' in u for u in users),'TongDanhGia':sum(len(u['DanhGia']) for u in users),'TongThongBao':sum(len(u['ThongBao']) for u in users),'TongMaBDSDaLuu':sum(len(u['BDSDaLuu']) for u in users)}
    (OUT/'ThongKeDuLieu.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps(summary,ensure_ascii=False))

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--count',type=int,default=10000);a=p.parse_args()
    if not 10000<=a.count<=50000:p.error('count phải từ 10000 đến 50000')
    generate(a.count)
