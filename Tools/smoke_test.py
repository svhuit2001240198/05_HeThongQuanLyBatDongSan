"""Kiểm thử HTTP trên app C# đang chạy. Tạo 1 tài khoản test mới; không xóa hay thay đổi User của nhóm.
Chạy sau khi seed/start: python Tools/smoke_test.py http://localhost:5156
"""
import http.cookiejar,html,re,sys,time,urllib.error,urllib.parse,urllib.request,uuid
BASE=(sys.argv[1] if len(sys.argv)>1 else 'http://localhost:5156').rstrip('/')
class Client:
    def __init__(self):self.http=urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
    def request(self,path,data=None):
        body=None if data is None else urllib.parse.urlencode(data).encode()
        try:
            with self.http.open(BASE+path,data=body,timeout=20) as r:return r.status,r.read().decode(),r.geturl()
        except urllib.error.HTTPError as e:return e.code,e.read().decode(),e.geturl()
    def post(self,path,data,token_path=None):
        status,page,_=self.request(token_path or path)
        assert status==200,(path,status)
        match=re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"',page)
        assert match,'Không tìm thấy antiforgery token'
        data['__RequestVerificationToken']=html.unescape(match[1])
        return self.request(path,data)
def check(ok,msg):
    assert ok,msg
    print('PASS:',msg)
guest=Client();status,page,url=guest.request('/Account/Profile')
check(status==200 and '/Account/Login' in url,'Khách chưa đăng nhập phải đến trang đăng nhập')
status,_,_=guest.request('/Account/Login',{'Email':'x@example.test','Password':'wrong'})
check(status==400,'POST thiếu antiforgery bị từ chối')
code=uuid.uuid4().hex[:12];email='smoke-'+code+'@example.test';pwd='Smoke@12345'
test=Client();status,page,url=test.post('/Account/Register',{'HoTen':'Kiểm Thử '+code,'Email':email,'SDT':'0901234567','Password':pwd,'ConfirmPassword':pwd,'VaiTro':'admin'})
check(status==200 and '/Account/Register' in url,'Form công khai không cho tự đăng ký admin')
status,page,url=test.post('/Account/Register',{'HoTen':'Kiểm Thử '+code,'Email':email,'SDT':'0901234567','Password':pwd,'ConfirmPassword':pwd,'VaiTro':'nguoi_mua'})
check(status==200 and '/Account/Register' not in url,'Đăng ký người mua thành công')
status,page,url=test.request('/Admin')
check('/Account/AccessDenied' in url,'Người mua không vào được quản trị')
status,page,url=test.post('/Account/Profile',{'HoTen':'Người Dùng '+code,'Email':email,'SDT':'0901234567','GioiTinh':'nam','VaiTro':'admin'})
check(status==200 and 'Người Dùng' in page,'Cập nhật hồ sơ thành công')
status,page,url=test.request('/Admin')
check('/Account/AccessDenied' in url,'Sửa hồ sơ không tự nâng quyền admin')
test.post('/Account/Logout',{},'/Account/Index')
status,page,url=test.post('/Account/Login',{'Email':email.upper(),'Password':pwd,'ReturnUrl':'https://example.com/'})
check(status==200 and url.startswith(BASE+'/Account'),'Đăng nhập chuẩn hóa email và chặn returnUrl ngoài hệ thống')
status,_,_=test.request('/Account/Save',{'maBDS':'BDS-DEMO-001'})
check(status==400,'Lưu tin thiếu antiforgery bị từ chối')
status,page,_=test.request('/Home/Details/BDS-DEMO-001')
token=html.unescape(re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"',page)[1])
for _ in range(2):test.request('/Account/Save',{'maBDS':'BDS-DEMO-001','returnUrl':'/Account/Favorites','__RequestVerificationToken':token})
status,page,_=test.request('/Account/Favorites')
check(status==200 and 'Căn hộ ánh sáng' in page and page.count('class="property-card"')==1,'Lưu tin và hiển thị BĐS tham chiếu')
status,page,url=test.post('/Account/ChangePassword',{'CurrentPassword':'wrong','NewPassword':'Changed@12345','ConfirmPassword':'Changed@12345'})
check('/Account/ChangePassword' in url and 'Mật khẩu hiện tại chưa đúng' in page,'Đổi mật khẩu phải kiểm tra mật khẩu cũ')
print('Hoàn thành. Tài khoản test được giữ lại:',email)
