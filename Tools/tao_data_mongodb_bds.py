import tkinter as tk
from tkinter import ttk, messagebox
from pymongo import MongoClient
from datetime import datetime, timedelta
import random

# =========================
# KET NOI MONGODB
# =========================
MONGO_URI = "mongodb://localhost:27017/"
DATABASE_NAME = "DoAn_NoSQL"
DEFAULT_PASSWORD = "12345678"

client = MongoClient(MONGO_URI)
db = client[DATABASE_NAME]

users_col = db["Users"]
bds_col = db["BDS"]

# =========================
# DU LIEU MAU
# =========================
HO = [
    "Nguyễn", "Trần", "Lê", "Phạm", "Hoàng",
    "Phan", "Vũ", "Võ", "Đặng", "Bùi",
    "Đỗ", "Ngô", "Dương", "Lý", "Trương"
]

TEN_DEM_NAM = [
    "Văn", "Hữu", "Đức", "Minh", "Quang",
    "Gia", "Thanh", "Công", "Anh", "Hoàng"
]

TEN_DEM_NU = [
    "Thị", "Ngọc", "Thu", "Kim", "Thanh",
    "Phương", "Hoài", "Minh", "Bích", "Diệu"
]

TEN_NAM = [
    "An", "Bình", "Cường", "Dũng", "Hải",
    "Hùng", "Khang", "Long", "Minh", "Nam",
    "Phúc", "Quân", "Sơn", "Thắng", "Tuấn",
    "Vinh", "Đạt", "Khôi", "Tùng", "Kiên"
]

TEN_NU = [
    "Anh", "Bình", "Chi", "Dung", "Hà",
    "Hạnh", "Hoa", "Lan", "Linh", "Mai",
    "Nga", "Ngọc", "Phương", "Quyên", "Thảo",
    "Trang", "Uyên", "Yến", "Vy", "Như"
]

DUONG = [
    "Nguyễn Huệ", "Lê Lợi", "Võ Văn Tần", "Nam Kỳ Khởi Nghĩa",
    "Điện Biên Phủ", "Cách Mạng Tháng 8", "Phạm Van Dong",
    "Nguyễn Văn Linh", "Hồ Diêu", "Trường Chinh", "Nguyễn Thị Minh Khai",
    "Nguyễn Thị Định", "Lý Thường Kiệt", "Nguyễn Trãi", "Nguyễn Hữu Cảnh", "Nguyễn Thị Thập",
    "Nguyễn Văn Cừ", "Lê Lợi", "Lê Đại", "Tân Kỳ Tân Quý", "Bình Quới", "Phan Văn Trị", "Nguyễn Thị Tú", "Nguyễn Văn Tạo",
    "Lê Lăng", "Lê Đại Hành"
]

PHUONG = [
    "Phường Bến Nghe", "Phường Đa Kao", "Phường 1", "Phường 2",
    "Phường 3", "Phường 4", "Phường 5", "Phường 6",
    "Phường 7", "Phường 8"
]

QUAN = [
    "Quận 1", "Quận 3", "Quận 5", "Quận 7",
    "Quận 10", "Quận 11", "Quận Bình Thạnh", "Quận Phú Nhuận"
]

THANH_PHO = [
    "Hồ Chí Minh", "Bình Dương", "Đồng Nai"
]

HUONG = [
    "Dong", "Tay", "Nam", "Bac",
    "Dong Nam", "Dong Bac", "Tay Nam", "Tay Bac"
]

TIEN_ICH = [
    "Ho boi", "Bai do xe", "Trung tam thuong mai",
    "An ninh 24/7", "Phong gym", "Cong vien",
    "Thang may", "San vuon"
]

LOAI_DAT = [
    "Dat o", "Dat nen du an", "Dat tho cu"
]

PHAP_LY = [
    "So hong", "So do", "Hop dong mua ban",
    "Giay chung nhan quyen su dung dat"
]

LOAI_THONG_BAO = [
    "tin_moi", "gia_bat_dong_san",
    "lich_hen", "he_thong"
]
# =========================
# HAM Tạo HO TEN NGẪU NHIÊN
# =========================
def tao_ho_ten():
    gioi_tinh = random.choice(["nam", "nu"])

    ho = random.choice(HO)

    if gioi_tinh == "nam":
        ten_dem = random.choice(TEN_DEM_NAM)
        ten = random.choice(TEN_NAM)
    else:
        ten_dem = random.choice(TEN_DEM_NU)
        ten = random.choice(TEN_NU)

    return {
        "HoTen": f"{ho} {ten_dem} {ten}",
        "GioiTinh": gioi_tinh
    }
# =========================
# HAM HO TRO
# =========================
def bo_dau_tieng_viet(text):
    """Chuyen chu co dau thanh khong dau"""
    bang_map = {
        'à': 'a', 'á': 'a', 'ả': 'a', 'ã': 'a', 'ạ': 'a',
        'ă': 'a', 'ằ': 'a', 'ắ': 'a', 'ẳ': 'a', 'ẵ': 'a', 'ặ': 'a',
        'â': 'a', 'ầ': 'a', 'ấ': 'a', 'ẩ': 'a', 'ẫ': 'a', 'ậ': 'a',
        'è': 'e', 'é': 'e', 'ẻ': 'e', 'ẽ': 'e', 'ẹ': 'e',
        'ê': 'e', 'ề': 'e', 'ế': 'e', 'ể': 'e', 'ễ': 'e', 'ệ': 'e',
        'ì': 'i', 'í': 'i', 'ỉ': 'i', 'ĩ': 'i', 'ị': 'i',
        'ò': 'o', 'ó': 'o', 'ỏ': 'o', 'õ': 'o', 'ọ': 'o',
        'ô': 'o', 'ồ': 'o', 'ố': 'o', 'ổ': 'o', 'ỗ': 'o', 'ộ': 'o',
        'ơ': 'o', 'ờ': 'o', 'ớ': 'o', 'ở': 'o', 'ỡ': 'o', 'ợ': 'o',
        'ù': 'u', 'ú': 'u', 'ủ': 'u', 'ũ': 'u', 'ụ': 'u',
        'ư': 'u', 'ừ': 'u', 'ứ': 'u', 'ử': 'u', 'ữ': 'u', 'ự': 'u',
        'ỳ': 'y', 'ý': 'y', 'ỷ': 'y', 'ỹ': 'y', 'ỵ': 'y',
        'đ': 'd',
        'À': 'A', 'Á': 'A', 'Ả': 'A', 'Ã': 'A', 'Ạ': 'A',
        'Ă': 'A', 'Ằ': 'A', 'Ắ': 'A', 'Ẳ': 'A', 'Ẵ': 'A', 'Ặ': 'A',
        'Â': 'A', 'Ầ': 'A', 'Ấ': 'A', 'Ẩ': 'A', 'Ẫ': 'A', 'Ậ': 'A',
        'È': 'E', 'É': 'E', 'Ẻ': 'E', 'Ẽ': 'E', 'Ẹ': 'E',
        'Ê': 'E', 'Ề': 'E', 'Ế': 'E', 'Ể': 'E', 'Ễ': 'E', 'Ệ': 'E',
        'Ì': 'I', 'Í': 'I', 'Ỉ': 'I', 'Ĩ': 'I', 'Ị': 'I',
        'Ò': 'O', 'Ó': 'O', 'Ỏ': 'O', 'Õ': 'O', 'Ọ': 'O',
        'Ô': 'O', 'Ồ': 'O', 'Ố': 'O', 'Ổ': 'O', 'Ỗ': 'O', 'Ộ': 'O',
        'Ơ': 'O', 'Ờ': 'O', 'Ớ': 'O', 'Ở': 'O', 'Ỡ': 'O', 'Ợ': 'O',
        'Ù': 'U', 'Ú': 'U', 'Ủ': 'U', 'Ũ': 'U', 'Ụ': 'U',
        'Ư': 'U', 'Ừ': 'U', 'Ứ': 'U', 'Ử': 'U', 'Ữ': 'U', 'Ự': 'U',
        'Ỳ': 'Y', 'Ý': 'Y', 'Ỷ': 'Y', 'Ỹ': 'Y', 'Ỵ': 'Y',
        'Đ': 'D',
    }
    return ''.join(bang_map.get(c, c) for c in text)


def tao_email(ho_ten, ma_user):
    ten = bo_dau_tieng_viet(ho_ten).lower().replace(" ", "")
    return ten + ma_user.lower() + "@gmail.com"


def tao_ngay_ngau_nhien():
    ngay = datetime.now() - timedelta(days=random.randint(0, 500))
    return ngay


# =========================
# TAO USER
# =========================
def tao_user(ma_user, vai_tro):
    thong_tin_ten = tao_ho_ten()

    ho_ten = thong_tin_ten["HoTen"]

    ngay_tao = tao_ngay_ngau_nhien()

    user = {
        "MaUser": ma_user,
        "HoTen": ho_ten,
        "GioiTinh": thong_tin_ten["GioiTinh"],
        "Email": tao_email(ho_ten, ma_user),
        "SDT": "09" + "".join(
            str(random.randint(0, 9))
            for _ in range(8)
        ),
        "MatKhau": DEFAULT_PASSWORD,
        "VaiTro": vai_tro,
        "NgayTao": ngay_tao
    }

    # Chi co moi_gioi moi co ThongTinMoiGioi
    if vai_tro == "moi_gioi":
        user["ThongTinMoiGioi"] = {
            "AnhDaiDien": "https://example.com/avatar/" + ma_user + ".jpg",
            "GioiThieu": "Moi gioi bat dong san chuyen nghiep.",
            "KinhNghiem": random.randint(1, 15),
            "KhuVucHoatDong": random.sample(
                ["Quận 1", "Quận 3", "Quận 7", "Quận 10",
                 "Quận Bình Thạnh", "Quận Phú Nhuận"],
                random.randint(2, 4)
            ),
            "TrangThai": random.choice([
                "dang_hoat_dong",
                "ngung_hoat_dong"
            ])
        }

        user["DanhGia"] = []

    # Danh sach BDS da luu
    user["BDSDaLuu"] = []

    # Danh sach thong bao
    user["ThongBao"] = []

    return user


def tao_users(so_luong):
    users = []

    vai_tro_list = [
        "nguoi_mua",
        "nguoi_ban",
        "moi_gioi",
        "admin"
    ]

    for i in range(1, so_luong + 1):

        ma_user = "U" + str(i).zfill(5)

        if i == 1:
            vai_tro = "admin"
        else:
            vai_tro = random.choices(
                vai_tro_list,
                weights=[40, 30, 25, 5],
                k=1
            )[0]

        users.append(
            tao_user(
                ma_user,
                vai_tro
            )
        )

    return users


# =========================
# TAO BDS
# =========================
def tao_dia_chi():
    return {
        "SoNha": str(random.randint(1, 999)),
        "Duong": random.choice(DUONG),
        "Phuong": random.choice(PHUONG),
        "Quan": random.choice(QUAN),
        "ThanhPho": random.choice(THANH_PHO)
    }


def tao_vi_tri():
    # Toa do khu vuc TP.HCM
    return {
        "KinhDo": round(random.uniform(106.62, 106.82), 6),
        "ViDo": round(random.uniform(10.73, 10.85), 6)
    }


def tao_hinh_anh(ma_bds):
    so_luong = random.randint(2, 5)
    ds = []

    for i in range(1, so_luong + 1):
        ds.append({
            "URL": f"/img/{random.randint(1, 10)}.jpg",
            "MoTa": "Hinh anh bat dong san " + str(i),
            "ThuTu": i
        })

    return ds


def tao_video(ma_bds):
    return [
        {
            "URL": "https://example.com/video/" + ma_bds + ".mp4",
            "MoTa": "Video thuc te bat dong san"
        }
    ]


def tao_thuoc_tinh(loai_bds):
    if loai_bds == "can_ho":
        return {
            "SoPhongNgu": random.randint(1, 4),
            "SoPhongTam": random.randint(1, 3),
            "Tang": random.randint(1, 30)
        }

    if loai_bds == "dat_nen":
        return {
            "ChieuDai": round(random.uniform(10, 30), 1),
            "ChieuRong": round(random.uniform(4, 15), 1),
            "LoaiDat": random.choice(LOAI_DAT),
            "PhapLy": random.choice(PHAP_LY)
        }

    return {
        "SoPhongNgu": random.randint(3, 7),
        "SoPhongTam": random.randint(2, 6),
        "SoTang": random.randint(2, 5),
        "CoHoBoi": random.choice([True, False]),
        "CoSanVuon": random.choice([True, False]),
        "ChoDoOto": random.choice([True, False])
    }


def tao_tieu_de(loai_bds):
    if loai_bds == "can_ho":
        return random.choice([
            "Can ho cao cap view dep",
            "Can ho trung tam thanh pho",
            "Can ho gan trung tam thuong mai",
            "Can ho 2 phong ngu tien nghi"
        ])

    if loai_bds == "dat_nen":
        return random.choice([
            "Dat nen vi tri dep",
            "Dat nen mat tien duong lon",
            "Dat nen khu dan cu",
            "Dat nen dau tu sinh loi"
        ])

    return random.choice([
        "Biet thu cao cap co san vuon",
        "Biet thu gan trung tam",
        "Biet thu co ho boi",
        "Biet thu khu dan cu cao cap"
    ])


def tao_lich_su_gia(gia_hien_tai, ma_nguoi):
    so_lan = random.randint(1, 3)
    ds = []

    gia = gia_hien_tai

    for i in range(so_lan):
        gia_moi = round(gia * random.uniform(0.85, 1.15), 0)

        ds.append({
            "GiaCu": gia,
            "GiaMoi": gia_moi,
            "ThoiGian": datetime.now() - timedelta(days=random.randint(1, 300)),
            "MaNguoiThayDoi": ma_nguoi
        })

        gia = gia_moi

    return ds


def tao_bds(ma_bds, chu_so_huu, moi_gioi):
    loai_bds = random.choice([
        "can_ho",
        "dat_nen",
        "biet_thu"
    ])

    loai_giao_dich = random.choice([
        "ban",
        "cho_thue"
    ])

    if loai_giao_dich == "ban":
        trang_thai = random.choice([
            "dang_ban",
            "da_dat_coc",
            "da_ban",
            "tam_an",
            "het_han"
        ])
    else:
        trang_thai = random.choice([
            "dang_cho_thue",
            "da_cho_thue",
            "tam_an",
            "het_han"
        ])

    if loai_bds == "can_ho":
        dien_tich = round(random.uniform(45, 180), 1)
        gia = random.randint(1500, 8000) * 1000000

    elif loai_bds == "dat_nen":
        dien_tich = round(random.uniform(60, 500), 1)
        gia = random.randint(1000, 15000) * 1000000

    else:
        dien_tich = round(random.uniform(150, 600), 1)
        gia = random.randint(5000, 50000) * 1000000

    ngay_dang = datetime.now() - timedelta(days=random.randint(1, 180))
    ngay_het_han = ngay_dang + timedelta(days=random.randint(30, 180))

    bds = {
        "MaBDS": ma_bds,
        "MaChuSoHuu": chu_so_huu,
        "MaMoiGioi": moi_gioi,
        "LoaiBDS": loai_bds,
        "TieuDe": tao_tieu_de(loai_bds),

        "DiaChi": tao_dia_chi(),

        "ViTri": tao_vi_tri(),

        "DienTich": dien_tich,
        "Gia": gia,
        "Huong": random.choice(HUONG),

        "MoTa": (
            "Bat dong san co vi tri dep, giao thong thuan tien, "
            "phu hop de o hoac dau tu."
        ),

        "HinhAnh": tao_hinh_anh(ma_bds),

        "Video": tao_video(ma_bds),

        "TienIch": random.sample(
            TIEN_ICH,
            random.randint(3, 6)
        ),

        "ThuocTinhRieng": tao_thuoc_tinh(loai_bds),

        "ThongTinGiaoDich": {
            "LoaiGiaoDich": loai_giao_dich,
            "TrangThai": trang_thai,
            "NgayDang": ngay_dang,
            "NgayHetHan": ngay_het_han
        },

        "LichSuGia": tao_lich_su_gia(gia, chu_so_huu)
    }

    return bds


# =========================
# TAO DU LIEU LIEN KET
# =========================
def tao_du_lieu_lien_ket(users, bds_list):
    # Lay danh sach moi gioi
    moi_gioi = [
        u for u in users
        if u["VaiTro"] == "moi_gioi"
    ]

    # Lay danh sach nguoi mua
    nguoi_mua = [
        u for u in users
        if u["VaiTro"] == "nguoi_mua"
    ]

    # Tao danh gia cho moi gioi
    for mg in moi_gioi:
        danh_gia = []

        for i in range(random.randint(1, 3)):
            if nguoi_mua:
                nguoi = random.choice(nguoi_mua)
            else:
                nguoi = random.choice(users)

            bds = random.choice(bds_list) if bds_list else None

            danh_gia.append({
                "MaUserDanhGia": nguoi["MaUser"],
                "MaBDS": bds["MaBDS"] if bds else None,
                "SoSao": random.randint(1, 5),
                "NoiDung": random.choice([
                    "Tu van rat nhiet tinh.",
                    "Nhan vien ho tro tot.",
                    "Thong tin ro rang.",
                    "Phuc vu chuyen nghiep."
                ]),
                "NgayDanhGia": tao_ngay_ngau_nhien()
            })

        mg["DanhGia"] = danh_gia

    # Tao BDS da luu va thong bao
    for user in users:
        if bds_list:
            so_luu = random.randint(0, min(4, len(bds_list)))
            ds_luu = random.sample(bds_list, so_luu)
            user["BDSDaLuu"] = [b["MaBDS"] for b in ds_luu]

        so_tb = random.randint(1, 3)

        for i in range(1, so_tb + 1):
            bds = random.choice(bds_list) if bds_list else None

            user["ThongBao"].append({
                "MaThongBao": "TB" + user["MaUser"] + "_" + str(i),
                "LoaiThongBao": random.choice(LOAI_THONG_BAO),
                "TieuDe": "Thong bao bat dong san",
                "NoiDung": "Ban co thong bao moi tu he thong.",
                "MaBDS": bds["MaBDS"] if bds else None,
                "MaLichHen": None,
                "DaDoc": random.choice([True, False]),
                "NgayTao": tao_ngay_ngau_nhien()
            })


# =========================
# TAO VA INSERT
# =========================
def tao_du_lieu():
    try:
        so_user = int(entry_users.get())
        so_bds = int(entry_bds.get())

        if so_user <= 0 or so_bds <= 0:
            messagebox.showwarning(
                "Loi",
                "So luong phai lon hon 0."
            )
            return

        # Tao users
        users = tao_users(so_user)

        # Lay nguoi ban
        nguoi_ban = [
            u for u in users
            if u["VaiTro"] == "nguoi_ban"
        ]

        # Neu khong co nguoi ban thi dung user bat ky
        if not nguoi_ban:
            nguoi_ban = users

        # Lay moi gioi
        moi_gioi = [
            u for u in users
            if u["VaiTro"] == "moi_gioi"
        ]

        bds_list = []

        for i in range(1, so_bds + 1):
            ma_bds = "BDS" + str(i).zfill(3)

            chu_so_huu = random.choice(nguoi_ban)["MaUser"]

            if moi_gioi and random.choice([True, True, False]):
                ma_moi_gioi = random.choice(moi_gioi)["MaUser"]
            else:
                ma_moi_gioi = None

            bds_list.append(
                tao_bds(
                    ma_bds,
                    chu_so_huu,
                    ma_moi_gioi
                )
            )

        # Tao du lieu lien ket
        tao_du_lieu_lien_ket(users, bds_list)

        # Insert MongoDB
        users_col.insert_many(users)
        bds_col.insert_many(bds_list)

        messagebox.showinfo(
            "Thanh cong",
            "Da tao du lieu thanh cong!\n\n"
            + "Users: " + str(len(users)) + "\n"
            + "BDS: " + str(len(bds_list)) + "\n\n"
            + "Database: " + DATABASE_NAME
        )

        cap_nhat_so_luong()

    except ValueError:
        messagebox.showerror(
            "Loi",
            "Vui long nhap so nguyen."
        )

    except Exception as e:
        messagebox.showerror(
            "Loi MongoDB",
            str(e)
        )


# =========================
# XOA DU LIEU
# =========================
def xoa_du_lieu():
    try:
        tra_loi = messagebox.askyesno(
            "Xac nhan",
            "Ban co chac muon xoa toan bo Users va BDS?"
        )

        if tra_loi:
            users_col.delete_many({})
            bds_col.delete_many({})

            messagebox.showinfo(
                "Thanh cong",
                "Da xoa toan bo du lieu."
            )

            cap_nhat_so_luong()

    except Exception as e:
        messagebox.showerror(
            "Loi",
            str(e)
        )


# =========================
# CAP NHAT SO LUONG
# =========================
def cap_nhat_so_luong():
    try:
        so_user = users_col.count_documents({})
        so_bds = bds_col.count_documents({})

        label_count.config(
            text="Users: " + str(so_user)
            + "        BDS: " + str(so_bds)
        )

    except Exception:
        label_count.config(
            text="Khong ket noi duoc MongoDB"
        )


# =========================
# KIEM TRA KET NOI
# =========================
def kiem_tra_ket_noi():
    try:
        client.admin.command("ping")

        messagebox.showinfo(
            "MongoDB",
            "Ket noi MongoDB thanh cong!\n"
            + "Database: " + DATABASE_NAME
        )

        cap_nhat_so_luong()

    except Exception as e:
        messagebox.showerror(
            "MongoDB",
            "Khong ket noi duoc MongoDB.\n\n"
            + str(e)
        )


# =========================
# GIAO DIEN
# =========================
root = tk.Tk()
root.title("MongoDB - Tao du lieu bat dong san")
root.geometry("650x480")
root.resizable(False, False)

style = ttk.Style()
style.configure(
    "TButton",
    font=("Arial", 11),
    padding=8
)

style.configure(
    "TLabel",
    font=("Arial", 11)
)

title = ttk.Label(
    root,
    text="CHUONG TRINH TAO DATA MONGODB",
    font=("Arial", 18, "bold")
)
title.pack(pady=20)

frame = ttk.Frame(root)
frame.pack(pady=10)

ttk.Label(
    frame,
    text="So luong Users:"
).grid(row=0, column=0, padx=10, pady=10)

entry_users = ttk.Entry(
    frame,
    width=20
)
entry_users.grid(row=0, column=1, padx=10, pady=10)
entry_users.insert(0, "20")

ttk.Label(
    frame,
    text="So luong BDS:"
).grid(row=1, column=0, padx=10, pady=10)

entry_bds = ttk.Entry(
    frame,
    width=20
)
entry_bds.grid(row=1, column=1, padx=10, pady=10)
entry_bds.insert(0, "30")

ttk.Button(
    root,
    text="TAO DATA",
    command=tao_du_lieu
).pack(pady=8)

ttk.Button(
    root,
    text="KIEM TRA KET NOI MONGODB",
    command=kiem_tra_ket_noi
).pack(pady=8)

ttk.Button(
    root,
    text="XOA TOAN BO DATA",
    command=xoa_du_lieu
).pack(pady=8)

label_count = ttk.Label(
    root,
    text="Users: 0        BDS: 0",
    font=("Arial", 12, "bold")
)
label_count.pack(pady=20)

ttk.Label(
    root,
    text="Mat khau mac dinh cua user: " + DEFAULT_PASSWORD + " (luu dang thuong)",
    font=("Arial", 10)
).pack(pady=5)

ttk.Label(
    root,
    text="Database: BDS_Marketplace | Collections: Users, BDS",
    font=("Arial", 10)
).pack(pady=5)

cap_nhat_so_luong()

root.mainloop()
