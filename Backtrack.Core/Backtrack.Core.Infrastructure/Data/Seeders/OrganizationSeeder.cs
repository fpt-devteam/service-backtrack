using Backtrack.Core.Application.Exceptions;
using Backtrack.Core.Application.Interfaces.Repositories;
using Backtrack.Core.Application.Usecases.Dev.JoinOrganization;
using Backtrack.Core.Application.Usecases.Organizations.CreateOrganization;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backtrack.Core.Infrastructure.Data.Seeders;

public static class OrganizationSeeder
{
    private sealed record OrgUserInfo(string Email, string Password, string DisplayName, string AvatarUrl);

    private sealed record OrgSeedData(
        string Name, string Slug,
        double Latitude, double Longitude,
        string DisplayAddress, string? ExternalPlaceId,
        string Phone, string ContactEmail, string TaxId,
        string LogoUrl, string? CoverImageUrl, string? LocationNote,
        string OpenTime, string CloseTime,
        OrgContractField[] RequiredFinderFields, OrgContractField[] RequiredOwnerFields,
        OrgUserInfo Admin, OrgUserInfo Staff);

    private static string Avatar(int u) => $"https://img.heroui.chat/image/avatar?w=400&h=400&u={u}";

    private static readonly OrgSeedData[] Organizations =
    [
        new(
            Name: "Ho Chi Minh City University of Technology", Slug: "hcmut",
            Latitude: 10.7724, Longitude: 106.6579,
            DisplayAddress: "268 Lý Thường Kiệt, Phường 14, Quận 10, TP.HCM", ExternalPlaceId: null,
            Phone: "(028) 38 651 670", ContactEmail: "info@hcmut.edu.vn", TaxId: "0301234561",
            LogoUrl: "https://img.favpng.com/16/20/22/ho-chi-minh-city-university-of-technology-vietnam-national-university-ho-chi-minh-city-hanoi-university-of-science-and-technology-university-of-technology-jamaica-png-favpng-BFnt5Mz1NGi1uScjMKVJDaA3u.jpg",
            CoverImageUrl: "https://ktgelectric.com/wp-content/uploads/2023/06/bk2.jpg",
            LocationNote: "Cổng chính đường Lý Thường Kiệt", OpenTime: "07:30", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.NationalId],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.NationalId],
            Admin: new("admin@hcmut.edu.vn", "Password@123", "Admin - HCMUT", Avatar(14)),
            Staff: new("staff@hcmut.edu.vn", "Password@123", "Staff - HCMUT", Avatar(15))),
        new(
            Name: "Da Nang City FPT University", Slug: "fptu-dn",
            Latitude: 15.9867, Longitude: 108.2638,
            DisplayAddress: "FPT Campus, Khu đô thị công nghệ FPT Đà Nẵng, Phường Hoà Hải, Quận Ngũ Hành Sơn, TP. Đà Nẵng.", ExternalPlaceId: null,
            Phone: "(0236) 37 377 88", ContactEmail: "info@fpt.edu.vn", TaxId: "0401234561",
            LogoUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR2VEpJOYuJlB-l7GEvROHM876ezvLc-cKiMQ&s",
            CoverImageUrl: "https://vinaconex25.com.vn/wp-content/uploads/2020/06/1.jpg",
            LocationNote: null, OpenTime: "07:30", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.NationalId],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.NationalId],
            Admin: new("admin-dn@fpt.edu.vn", "Password@123", "Admin - FPTU", Avatar(14)),
            Staff: new("staff-dn@fpt.edu.vn", "Password@123", "Staff - FPTU", Avatar(15))),
        new(
            Name: "Da Nang City Duy Tan University", Slug: "dtu-dn",
            Latitude: 16.0697, Longitude: 108.2030,
            DisplayAddress: "254 Nguyễn Văn Linh, phường Thạc Gián, quận Hải Châu, TP. Đà Nẵng.", ExternalPlaceId: null,
            Phone: "(0236) 38 220 68", ContactEmail: "info@dtu.edu.vn", TaxId: "0401234562",
            LogoUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQMsaMc1Wd_AWM1RwFAJXWZEZLhoTJJYzNNug&s",
            CoverImageUrl: "https://vcdn1-vnexpress.vnecdn.net/2025/06/03/01ce15ff-be8c-4b21-a15e-77f1b4-2897-8590-1748918624.jpg?w=500&h=300&q=100&dpr=1&fit=crop&s=89UKIWU97RXgRcL1aH9LcA",
            LocationNote: null, OpenTime: "07:30", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.NationalId],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.NationalId],
            Admin: new("admin-dn@dtu.edu.vn", "Password@123", "Admin - DTU", Avatar(14)),
            Staff: new("staff-dn@dtu.edu.vn", "Password@123", "Staff - DTU", Avatar(15))),
        new(
            Name: "School of medicine and pharmacy - The University of Danang", Slug: "udn-dn",
            Latitude: 15.9877, Longitude: 108.2461,
            DisplayAddress: "Đ. Lưu Quang Vũ, Hoà Hải, Ngũ Hành Sơn, Đà Nẵng.", ExternalPlaceId: null,
            Phone: "(0236) 37 655 55", ContactEmail: "info@udn.edu.vn", TaxId: "0401234563",
            LogoUrl: "https://thongtintuyensinh.edu.vn/wp-content/uploads/2023/11/khoa-y-duoc-dai-hoc-da-nang-the-university-of-danang-school-of-medicine-and-pharmacy-0mfmzdtj.jpeg",
            CoverImageUrl: "https://cdn2.tuoitre.vn/zoom/700_438/471584752817336320/2025/8/22/thumb-img5056-17558508940421728943822-1755850997170444379830-213-0-1089-1401-crop-17558510116341765785382.jpeg",
            LocationNote: null, OpenTime: "07:30", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.NationalId],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.NationalId],
            Admin: new("admin-dn@udn.edu.vn", "Password@123", "Admin - UDN", Avatar(14)),
            Staff: new("staff-dn@udn.edu.vn", "Password@123", "Staff - UDN", Avatar(15))),

        new(
            Name: "VNUHCM - University of Science", Slug: "hcmus",
            Latitude: 10.7628, Longitude: 106.6823,
            DisplayAddress: "227 Nguyễn Văn Cừ, Phường 4, Quận 5, TP.HCM", ExternalPlaceId: null,
            Phone: "(028) 38 353 193", ContactEmail: "bantin@hcmus.edu.vn", TaxId: "0301234562",
            LogoUrl: "https://upload.wikimedia.org/wikipedia/commons/1/16/Logo-KHTN.jpg",
            CoverImageUrl: "https://edus3.leaderbook.com/prod/upload/img/66ea4373aa37860051d3799c-truong-dai-hoc-khoa-hoc-tu-nhien-dai-hoc-quoc-gia-tp.hcm.png",
            LocationNote: null, OpenTime: "08:00", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.NationalId],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.NationalId],
            Admin: new("admin@hcmus.edu.vn", "Password@123", "Admin - HCMUS", Avatar(16)),
            Staff: new("staff@hcmus.edu.vn", "Password@123", "Staff - HCMUS", Avatar(17))),

        new(
            Name: "University of Economics Ho Chi Minh City", Slug: "ueh",
            Latitude: 10.7828, Longitude: 106.6917,
            DisplayAddress: "59C Nguyễn Đình Chiểu, Phường Võ Thị Sáu, Quận 3, TP.HCM", ExternalPlaceId: null,
            Phone: "(028) 38 295 299", ContactEmail: "info@ueh.edu.vn", TaxId: "0301234563",
            LogoUrl: "https://ueh.edu.vn/img/logo.png", CoverImageUrl: "https://ueh.edu.vn/img/cover.jpg",
            LocationNote: "Cơ sở A", OpenTime: "07:30", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.Phone],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.Phone],
            Admin: new("admin@ueh.edu.vn", "Password@123", "Admin - UEH", Avatar(18)),
            Staff: new("staff@ueh.edu.vn", "Password@123", "Staff - UEH", Avatar(19))),

        new(
            Name: "National Economics University", Slug: "neu",
            Latitude: 21.0000, Longitude: 105.8441,
            DisplayAddress: "207 Giải Phóng, Đồng Tâm, Hai Bà Trưng District, Hanoi.", ExternalPlaceId: null,
            Phone: "(024) 36 280 280", ContactEmail: "info@neu.edu.vn", TaxId: "0101234561",
            LogoUrl: "https://startupwheel.vn/wp-content/uploads/2021/04/neu-logo.jpg", CoverImageUrl: "https://cdn2.tuoitre.vn/471584752817336320/2024/11/15/snapedit1731663957416-1731663980474276682980.jpeg",
            LocationNote: null, OpenTime: "07:30", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.Phone],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.Phone],
            Admin: new("admin@neu.edu.vn", "Password@123", "Admin - NEU", Avatar(24)),
            Staff: new("staff@neu.edu.vn", "Password@123", "Staff - NEU", Avatar(25))),
        
        new(
            Name: "FPT University Hanoi", Slug: "fpt-hanoi",
            Latitude: 21.0135, Longitude: 105.5255,
            DisplayAddress: "Hoa Lac Hi-tech Park, km 29, Đại lộ, Hòa Lạc, Hà Nội", ExternalPlaceId: null,
            Phone: "(024) 73 005 588", ContactEmail: "info@fpt.edu.vn", TaxId: "0101234562",
            LogoUrl: "https://is1-ssl.mzstatic.com/image/thumb/Purple126/v4/25/a3/fd/25a3fd67-a758-8e6f-99e2-c0c2f629d04c/AppIcon-0-0-1x_U007emarketing-0-0-0-7-0-0-sRGB-0-0-0-GLES2_U002c0-512MB-85-220-0-0.png/1200x630wa.png", CoverImageUrl: "https://cafebiz.cafebizcdn.vn/162123310254002176/2026/1/16/4-1-1768535887034607574230-1768536855419-1768536860977361438902.jpg",
            LocationNote: null, OpenTime: "07:30", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.Phone],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.Phone],
            Admin: new("admin-hn@fpt.edu.vn", "Password@123", "Admin - FPT", Avatar(26)),
            Staff: new("staff-hn@fpt.edu.vn", "Password@123", "Staff - FPT", Avatar(27))),

        new(
            Name: "VinUniversity", Slug: "vinuni",
            Latitude: 20.9893, Longitude: 105.9436,
            DisplayAddress: "Vinhomes Ocean Park, Gia Lâm, Hà Nội", ExternalPlaceId: null,
            Phone: "(024) 7108 9779", ContactEmail: "daotao@vinuni.edu.vn", TaxId: "0101234563",
            LogoUrl: "https://international-sustainable-campus-network.org/wp-content/uploads/2023/12/VinUniversity_logo.png",
            CoverImageUrl: "https://ircdn.vingroup.net/storage/uploads/0_Tintuchoatdong/2018/Vinuni/Vinuni%201411_1.jpg",
            LocationNote: null, OpenTime: "08:00", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.NationalId],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.NationalId],
            Admin: new("admin@vinuni.edu.vn", "Password@123", "Admin - VinUniversity", Avatar(22)),
            Staff: new("staff@vinuni.edu.vn", "Password@123", "Staff - VinUniversity", Avatar(23))),

        new(
            Name: "FPT University Can Tho", Slug: "fpt-ct",
            Latitude: 10.0125, Longitude: 105.7330,
            DisplayAddress: " 600 Nguyen Van Cu, An Binh Ward, Ninh Kieu District, Can Tho City", ExternalPlaceId: null,
            Phone: "(0292) 37 305 88", ContactEmail: "daotao@fpt.edu.vn", TaxId: "1801234561",
            LogoUrl: "https://khoahoc.vietjack.com/upload/images/admission/5961185/a23-1757664602.png",
            CoverImageUrl: "https://anzschool.com/wp-content/uploads/2023/11/dai-hoc-fpt-can-tho.jpg",
            LocationNote: null, OpenTime: "08:00", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.NationalId],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.NationalId],
            Admin: new("admin-ct@fpt.edu.vn", "Password@123", "Admin - FPT University Can Tho", Avatar(22)),
            Staff: new("staff-ct@fpt.edu.vn", "Password@123", "Staff - FPT University Can Tho", Avatar(23))),

        new(
            Name: "Can Tho University of Medicine and Pharmacy", Slug: "ctump",
            Latitude: 10.0369, Longitude: 105.7556,
            DisplayAddress: " Số 179 Đường Nguyễn Văn Cừ, Phường Tân An, Thành phố Cần Thơ", ExternalPlaceId: null,
            Phone: "(0292) 37 220 05", ContactEmail: "daotao@ctump.edu.vn", TaxId: "1801234562",
            LogoUrl: "https://diendantructuyen.com/wp-content/uploads/2025/10/logo-dai-hoc-y-duoc-can-tho-8.jpg",
            CoverImageUrl: "https://vcdn1-vnexpress.vnecdn.net/2023/05/10/3245-1663237538-5758-1683715068.jpg?w=680&h=0&q=100&dpr=2&fit=crop&s=vF1fiVXI_oUiziYQ0Ikk5A",
            LocationNote: null, OpenTime: "08:00", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.NationalId],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.NationalId],
            Admin: new("admin-ct@ctump.edu.vn", "Password@123", "Admin - Can Tho University of Medicine and Pharmacy", Avatar(22)),
            Staff: new("staff-ct@ctump.edu.vn", "Password@123", "Staff - Can Tho University of Medicine and Pharmacy", Avatar(23))),

        new(
            Name: "Can Tho University", Slug: "ctu",
            Latitude: 10.0305, Longitude: 105.7686,
            DisplayAddress: " 3/2 Street, Xuan Khanh Ward, Ninh Kieu District,Thành phố Cần Thơ", ExternalPlaceId: null,
            Phone: "(0292) 37 316 66", ContactEmail: "daotao@ctu.edu.vn", TaxId: "1801234563",
            LogoUrl: "https://yu.ctu.edu.vn/images/upload/article/2020/03/0305-logo-ctu.png",
            CoverImageUrl: "https://en.ctu.edu.vn/images/upload/images/2024/RLC1.jpg",
            LocationNote: null, OpenTime: "08:00", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.NationalId],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.NationalId],
            Admin: new("admin-ct@ctu.edu.vn", "Password@123", "Admin - Can Tho University", Avatar(22)),
            Staff: new("staff-ct@ctu.edu.vn", "Password@123", "Staff - Can Tho University", Avatar(23))),
        new(
            Name: "FPT University Quy Nhon", Slug: "fpt-qn",
            Latitude: 13.8039, Longitude: 109.2191,
            DisplayAddress: "  Lot B1-57, An Phu Thinh New Urban Area, Nhon Binh and Dong Da Wards, Quy Nhon.", ExternalPlaceId: null,
            Phone: "(0256) 38 991 88", ContactEmail: "daotao@fpt.edu.vn", TaxId: "4101234561",
            LogoUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTQ1hp4p13cD2m6H0Bf32s2aHKzCe4hEqYW7g&s",
            CoverImageUrl: "https://fpt.com/Images/images/Screen-Shot-2019-12-20-at-9_01_37-AM.png",
            LocationNote: null, OpenTime: "08:00", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.NationalId],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.NationalId],
            Admin: new("admin-qn@fpt.edu.vn", "Password@123", "Admin - FPT University Quy Nhon", Avatar(22)),
            Staff: new("staff-qn@fpt.edu.vn", "Password@123", "Staff - FPT University Quy Nhon", Avatar(23))),

        new(
            Name: "University of Medicine and Pharmacy at HCMC", Slug: "ump-hcm",
            Latitude: 10.7554, Longitude: 106.6633,
            DisplayAddress: "217 Hồng Bàng, Phường 11, Quận 5, TP.HCM", ExternalPlaceId: null,
            Phone: "(028) 38 554 269", ContactEmail: "daotao@ump.edu.vn", TaxId: "0301234572",
            LogoUrl: "https://upload.wikimedia.org/wikipedia/commons/7/71/Logo_UEH_xanh.png",
            CoverImageUrl: "https://winhouse.vn/wp-content/uploads/2021/02/4-scaled.jpg",
            LocationNote: null, OpenTime: "08:00", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.NationalId],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.NationalId],
            Admin: new("admin@ump.edu.vn", "Password@123", "Admin - UMP", Avatar(20)),
            Staff: new("staff@ump.edu.vn", "Password@123", "Staff - UMP", Avatar(21))),

        new(
            Name: "Foreign Trade University Campus II", Slug: "ftu2",
            Latitude: 10.8042, Longitude: 106.7144,
            DisplayAddress: "15 Đường D5, Phường 25, Bình Thạnh, TP.HCM", ExternalPlaceId: null,
            Phone: "(028) 35 127 254", ContactEmail: "qhqt.cs2@ftu.edu.vn", TaxId: "0301234565",
            LogoUrl: "https://upload.wikimedia.org/wikipedia/en/c/cb/Foreign_Trade_University_logo.jpg",
            CoverImageUrl: "https://api.upm.vn//uploads/university/cover/1628700846-qsJlan8R1KfpB2iT.jpg",
            LocationNote: null, OpenTime: "08:00", CloseTime: "17:30",
            RequiredFinderFields: [OrgContractField.Email],
            RequiredOwnerFields:  [OrgContractField.Email],
            Admin: new("admin@ftu.edu.vn", "Password@123", "Admin - FTU2", Avatar(22)),
            Staff: new("staff@ftu.edu.vn", "Password@123", "Staff - FTU2", Avatar(23))),

        new(
            Name: "Ho Chi Minh City University of Education", Slug: "hcmue",
            Latitude: 10.7601, Longitude: 106.6782,
            DisplayAddress: "280 An Dương Vương, Phường 4, Quận 5, TP.HCM", ExternalPlaceId: null,
            Phone: "(028) 38 352 020", ContactEmail: "phongdaotao@hcmue.edu.vn", TaxId: "0301234566",
            LogoUrl: "https://cdn.haitrieu.com/wp-content/uploads/2022/02/Logo-DH-Su-Pham-TPHCM-HCMUE.png",
            CoverImageUrl: "https://xdcs.cdnchinhphu.vn/446259493575335936/2024/7/19/su-1721386673248568422887.jpg",
            LocationNote: null, OpenTime: "07:30", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.NationalId],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.NationalId],
            Admin: new("admin@hcmue.edu.vn", "Password@123", "Admin - HCMUE", Avatar(24)),
            Staff: new("staff@hcmue.edu.vn", "Password@123", "Staff - HCMUE", Avatar(25))),

        new(
            Name: "Sai Gon University", Slug: "sgu",
            Latitude: 10.759, Longitude: 106.681,
            DisplayAddress: "273 An Dương Vương, Phường 3, Quận 5, TP.HCM", ExternalPlaceId: null,
            Phone: "(028) 38 354 409", ContactEmail: "p_daotao@sgu.edu.vn", TaxId: "0301234567",
            LogoUrl: "https://etc.sgu.edu.vn/wp-content/uploads/2018/11/SGU-LOGO.png",
            CoverImageUrl: "https://daotao.sgu.edu.vn/images/headers/DSC06478_1.jpg",
            LocationNote: "Cơ sở chính", OpenTime: "08:00", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.Phone],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.Phone],
            Admin: new("admin@sgu.edu.vn", "Password@123", "Admin - Sai Gon University", Avatar(26)),
            Staff: new("staff@sgu.edu.vn", "Password@123", "Staff - Sai Gon University", Avatar(27))),

        new(
            Name: "HCMC University of Technology and Education", Slug: "hcmute",
            Latitude: 10.8507, Longitude: 106.7724,
            DisplayAddress: "1 Võ Văn Ngân, Phường Linh Chiểu, Thủ Đức, TP.HCM", ExternalPlaceId: null,
            Phone: "(028) 38 968 641", ContactEmail: "ptc@hcmute.edu.vn", TaxId: "0301234568",
            LogoUrl: "https://careerhub.hcmute.edu.vn/assets/img/logo/ute_logo.png",
            CoverImageUrl: "https://hcmute.edu.vn/Resources/Images/SubDomain/HomePage/tin%20tuc/Nam%202025/Vinh%20danh/Vinh%20danh%204.jpg",
            LocationNote: null, OpenTime: "07:00", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.Phone],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.Phone],
            Admin: new("admin@hcmute.edu.vn", "Password@123", "Admin - HCMUTE", Avatar(28)),
            Staff: new("staff@hcmute.edu.vn", "Password@123", "Staff - HCMUTE", Avatar(29))),

        new(
            Name: "FPT University HCMC", Slug: "fptu-hcmc",
            Latitude: 10.8417, Longitude: 106.8100,
            DisplayAddress: "Lô E2a-7, Đường D1, Khu Công nghệ cao, Phường Tăng Nhơn Phú A, TP.HCM",
            ExternalPlaceId: "239813160",
            Phone: "(028) 73 005 588", ContactEmail: "tuyensinhhcm@fpt.edu.vn", TaxId: "0319463316-001",
            LogoUrl: "https://daihoc.fpt.edu.vn/wp-content/themes/fpt-university/assets/images/logo.png",
            CoverImageUrl: "https://daihoc.fpt.edu.vn/wp-content/uploads/2025/12/P.Zo-9862-scaled.jpg",
            LocationNote: "Khu công nghệ cao", OpenTime: "08:00", CloseTime: "17:30",
            RequiredFinderFields: [OrgContractField.Email],
            RequiredOwnerFields:  [OrgContractField.Email],
            Admin: new("admin@fpt.edu.vn", "Password@123", "Admin - FPT University HCMC", Avatar(30)),
            Staff: new("staff@fpt.edu.vn", "Password@123", "Staff - FPT University HCMC", Avatar(31))),

        new(
            Name: "Ton Duc Thang University", Slug: "tdtu",
            Latitude: 10.7326, Longitude: 106.6997,
            DisplayAddress: "19 Nguyễn Hữu Thọ, Phường Tân Phong, Quận 7, TP.HCM", ExternalPlaceId: null,
            Phone: "(028) 37 755 035", ContactEmail: "tuvan@tdtu.edu.vn", TaxId: "0301234569",
            LogoUrl: "https://upload.wikimedia.org/wikipedia/vi/1/1b/T%C4%90T_logo.png",
            CoverImageUrl: "https://khanhhoa.tdtu.edu.vn/sites/nhatrang/files/nhatrang/9_0.jpg",
            LocationNote: null, OpenTime: "07:30", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.NationalId],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.NationalId],
            Admin: new("admin@tdtu.edu.vn", "Password@123", "Admin - TDTU", Avatar(32)),
            Staff: new("staff@tdtu.edu.vn", "Password@123", "Staff - TDTU", Avatar(33))),

        new(
            Name: "University of Architecture Ho Chi Minh City", Slug: "uah",
            Latitude: 10.7818, Longitude: 106.6919,
            DisplayAddress: "196 Pasteur, Phường Võ Thị Sáu, Quận 3, TP.HCM", ExternalPlaceId: null,
            Phone: "(028) 38 222 748", ContactEmail: "phongdaotao@uah.edu.vn", TaxId: "0301234570",
            LogoUrl: "https://upload.wikimedia.org/wikipedia/vi/8/80/Logo_of_University_of_Architecture_Ho_Chi_Minh_City_red_version.png",
            CoverImageUrl: "https://upload.wikimedia.org/wikipedia/commons/5/59/HCMC_Dai_Hoc_Kien_Truc_01.JPG",
            LocationNote: "Cơ sở Pasteur", OpenTime: "07:30", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.Phone],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.Phone],
            Admin: new("admin@uah.edu.vn", "Password@123", "Admin - UAH", Avatar(34)),
            Staff: new("staff@uah.edu.vn", "Password@123", "Staff - UAH", Avatar(35))),

        new(
            Name: "VNUHCM - University of Social Sciences and Humanities", Slug: "ussh",
            Latitude: 10.7853, Longitude: 106.7028,
            DisplayAddress: "10-12 Đinh Tiên Hoàng, Phường Bến Nghé, Quận 1, TP.HCM", ExternalPlaceId: null,
            Phone: "(028) 38 293 828", ContactEmail: "phongdaotao@hcmussh.edu.vn", TaxId: "0301234571",
            LogoUrl: "https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Logo_Tr%C6%B0%E1%BB%9Dng_%C4%90%E1%BA%A1i_h%E1%BB%8Dc_Khoa_h%E1%BB%8Dc_X%C3%A3_h%E1%BB%99i_v%C3%A0_Nh%C3%A2n_v%C4%83n%2C_%C4%90%E1%BA%A1i_h%E1%BB%8Dc_Qu%E1%BB%91c_gia_Th%C3%A0nh_ph%E1%BB%91_H%E1%BB%93_Ch%C3%AD_Minh.svg/250px-Logo_Tr%C6%B0%E1%BB%9Dng_%C4%90%E1%BA%A1i_h%E1%BB%8Dc_Khoa_h%E1%BB%8Dc_X%C3%A3_h%E1%BB%99i_v%C3%A0_Nh%C3%A2n_v%C4%83n%2C_%C4%90%E1%BA%A1i_h%E1%BB%8Dc_Qu%E1%BB%91c_gia_Th%C3%A0nh_ph%E1%BB%91_H%E1%BB%93_Ch%C3%AD_Minh.svg.png",
            CoverImageUrl: "https://hcmussh.edu.vn/img/news/56257806.JPG?t=56257807",
            LocationNote: "Cơ sở Đinh Tiên Hoàng", OpenTime: "08:00", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.NationalId],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.NationalId],
            Admin: new("admin@hcmussh.edu.vn", "Password@123", "Admin - USSH", Avatar(36)),
            Staff: new("staff@hcmussh.edu.vn", "Password@123", "Staff - USSH", Avatar(37))),

        new(
            Name: "RMIT University Vietnam", Slug: "rmit-hcm",
            Latitude: 10.7303, Longitude: 106.6961,
            DisplayAddress: "702 Nguyễn Văn Linh, Phường Tân Phong, Quận 7, TP.HCM", ExternalPlaceId: null,
            Phone: "(028) 37 761 300", ContactEmail: "enquiries@rmit.edu.vn", TaxId: "0302324561",
            LogoUrl: "https://upload.wikimedia.org/wikipedia/commons/5/51/RMIT_University_Logo.svg",
            CoverImageUrl: "https://cdn2.tuoitre.vn/zoom/700_525/471584752817336320/2025/5/29/rmit-1748522121074473030711-213-0-998-1500-crop-1748522173982199998048.jpg",
            LocationNote: "Cơ sở Nam Sài Gòn (SGS)", OpenTime: "08:00", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.Phone, OrgContractField.NationalId],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.Phone, OrgContractField.NationalId],
            Admin: new("admin@rmit.edu.vn", "Password@123", "Admin - RMIT Vietnam", Avatar(38)),
            Staff: new("staff@rmit.edu.vn", "Password@123", "Staff - RMIT Vietnam", Avatar(39))),

        new(
            Name: "Fulbright University Vietnam", Slug: "fulbright",
            Latitude: 10.7259, Longitude: 106.7214,
            DisplayAddress: "105 Tôn Dật Tiên, Phường Tân Phú, Quận 7, TP.HCM", ExternalPlaceId: null,
            Phone: "(028) 73 032 255", ContactEmail: "info@fulbright.edu.vn", TaxId: "0302324562",
            LogoUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRGwDSW6mudp0v8JajTVGD0qI7Zq5WxFf8UHg&s",
            CoverImageUrl: "https://newtecons.vn/wp-content/uploads/2021/08/fulbright.jpg",
            LocationNote: "Cơ sở Crescent", OpenTime: "08:30", CloseTime: "17:30",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.Phone],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.Phone],
            Admin: new("admin@fulbright.edu.vn", "Password@123", "Admin - Fulbright", Avatar(40)),
            Staff: new("staff@fulbright.edu.vn", "Password@123", "Staff - Fulbright", Avatar(41))),

        new(
            Name: "Swinburne University of Technology", Slug: "swinburne-hcm",
            Latitude: 10.8123, Longitude: 106.6667,
            DisplayAddress: "A35 Bạch Đằng, Phường 2, Tân Bình, TP.HCM", ExternalPlaceId: null,
            Phone: "1900 3205", ContactEmail: "swinburne@fe.edu.vn", TaxId: "0302324563",
            LogoUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQ340cwIdcfFios_ZdaHEe91VX7EwUd7ye4iQ&s",
            CoverImageUrl: "https://www.aeccglobal.lk/images/2021/08/19/swinburne-university-mob.jpg",
            LocationNote: null, OpenTime: "08:00", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email],
            RequiredOwnerFields:  [OrgContractField.Email],
            Admin: new("admin@swinburne-vn.edu.vn", "Password@123", "Admin - Swinburne HCMC", Avatar(42)),
            Staff: new("staff@swinburne-vn.edu.vn", "Password@123", "Staff - Swinburne HCMC", Avatar(43))),

        new(
            Name: "Greenwich Viet Nam", Slug: "greenwich-hcm",
            Latitude: 10.8012, Longitude: 106.6543,
            DisplayAddress: "20 Cộng Hòa, Phường 12, Tân Bình, TP.HCM", ExternalPlaceId: null,
            Phone: "(028) 73 002 266", ContactEmail: "hcm@greenwich.edu.vn", TaxId: "0302324564",
            LogoUrl: "https://kyluc.vn/Userfiles/Upload/images/2022-Greenwich-Eng.jpg",
            CoverImageUrl: "https://greenwich.edu.vn/wp-content/uploads/2022/10/Welcome-1.jpg",
            LocationNote: null, OpenTime: "08:00", CloseTime: "17:30",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.Phone],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.Phone],
            Admin: new("admin@greenwich.edu.vn", "Password@123", "Admin - Greenwich HCMC", Avatar(44)),
            Staff: new("staff@greenwich.edu.vn", "Password@123", "Staff - Greenwich HCMC", Avatar(45))),

        new(
            Name: "Western Sydney University Vietnam", Slug: "western-sydney-vn",
            Latitude: 10.7834, Longitude: 106.6946,
            DisplayAddress: "17 Phạm Ngọc Thạch, Phường Võ Thị Sáu, Quận 3, TP.HCM", ExternalPlaceId: null,
            Phone: "(028) 39 305 293", ContactEmail: "westernsydney@isb.edu.vn", TaxId: "0302324565",
            LogoUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQFy1jO_pe_QvybLOlHqeF9xovPWwSbRvIOGw&s",
            CoverImageUrl: "https://sunrisevietnam.com/sites/default/files/anh_bai_viet/Western%20Sydney%20University%202.jpg",
            LocationNote: "Viện ISB - Tòa nhà Phạm Ngọc Thạch", OpenTime: "08:00", CloseTime: "17:00",
            RequiredFinderFields: [OrgContractField.Email, OrgContractField.Phone],
            RequiredOwnerFields:  [OrgContractField.Email, OrgContractField.Phone],
            Admin: new("admin@westernsydney.edu.vn", "Password@123", "Admin - Western Sydney VN", Avatar(46)),
            Staff: new("staff@westernsydney.edu.vn", "Password@123", "Staff - Western Sydney VN", Avatar(47))),
    ];

    public static async Task SeedAsync(
        ApplicationDbContext db,
        ISender mediator,
        IOrganizationRepository orgRepository,
        ISubscriptionRepository subscriptionRepository,
        ILogger logger,
        CancellationToken ct = default)
    {
        var created = new List<string>();
        var skipped = new List<string>();

        foreach (var org in Organizations)
        {
            try
            {
                var wasCreated = await SeedOneAsync(db, org, mediator, orgRepository, subscriptionRepository, logger, ct);
                (wasCreated ? created : skipped).Add(org.Slug);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to seed organization {Slug} — skipping.", org.Slug);
                skipped.Add(org.Slug);
            }
        }

        logger.LogInformation(
            "Organizations — created {Created}, skipped {Skipped}. Created: [{CreatedNames}] Skipped: [{SkippedNames}]",
            created.Count, skipped.Count,
            string.Join(", ", created),
            string.Join(", ", skipped));
    }

    private static async Task<bool> SeedOneAsync(
        ApplicationDbContext db, OrgSeedData org, ISender mediator,
        IOrganizationRepository orgRepository, ISubscriptionRepository subscriptionRepository,
        ILogger logger, CancellationToken ct)
    {
        var adminResult = await DataSeederHelper.SeedUserAsync(
            db, org.Admin.Email, org.Admin.Password, org.Admin.DisplayName, mediator, logger, org.Admin.AvatarUrl, ct);

        var staffResult = await DataSeederHelper.SeedUserAsync(
            db, org.Staff.Email, org.Staff.Password, org.Staff.DisplayName, mediator, logger, org.Staff.AvatarUrl, ct);

        var existing = await orgRepository.GetBySlugAsync(org.Slug, ct);

        Guid orgId;
        bool wasCreated;
        if (existing is not null)
        {
            orgId = existing.Id;
            wasCreated = false;
        }
        else
        {
            var result = await mediator.Send(new CreateOrganizationCommand
            {
                UserId                       = adminResult.User.Id,
                Name                         = org.Name,
                Slug                         = org.Slug,
                Location                     = new GeoPoint(org.Latitude, org.Longitude),
                DisplayAddress               = org.DisplayAddress,
                ExternalPlaceId              = org.ExternalPlaceId,
                Phone                        = org.Phone,
                ContactEmail                 = org.ContactEmail,
                IndustryType                 = "University",
                TaxIdentificationNumber      = org.TaxId,
                LogoUrl                      = org.LogoUrl,
                CoverImageUrl                = org.CoverImageUrl,
                LocationNote                 = org.LocationNote,
                BusinessHours                = BuildBusinessHours(org.OpenTime, org.CloseTime),
                RequiredFinderContractFields = [.. org.RequiredFinderFields],
                RequiredOwnerContractFields  = [.. org.RequiredOwnerFields],
            }, ct);

            orgId = result.Id;
            wasCreated = true;
        }

        await JoinStaffAsync(orgId, staffResult.User.Id, mediator, logger, ct);
        return wasCreated;
    }

    private static async Task JoinStaffAsync(
        Guid orgId, string staffUserId, ISender mediator, ILogger logger, CancellationToken ct)
    {
        try
        {
            await mediator.Send(new DevJoinOrganizationCommand
            {
                UserId         = staffUserId,
                OrganizationId = orgId,
                Role           = MembershipRole.OrgStaff,
            }, ct);

            logger.LogInformation("Staff {UserId} joined org {OrgId}.", staffUserId, orgId);
        }
        catch (ConflictException)
        {
            logger.LogInformation("Staff {UserId} already member of org {OrgId} — skipping.", staffUserId, orgId);
        }
    }

    private static List<DailySchedule> BuildBusinessHours(string openTime, string closeTime) =>
    [
        new DailySchedule { Day = WeekDay.Monday,    IsClosed = false, OpenTime = openTime, CloseTime = closeTime },
        new DailySchedule { Day = WeekDay.Tuesday,   IsClosed = false, OpenTime = openTime, CloseTime = closeTime },
        new DailySchedule { Day = WeekDay.Wednesday, IsClosed = false, OpenTime = openTime, CloseTime = closeTime },
        new DailySchedule { Day = WeekDay.Thursday,  IsClosed = false, OpenTime = openTime, CloseTime = closeTime },
        new DailySchedule { Day = WeekDay.Friday,    IsClosed = false, OpenTime = openTime, CloseTime = closeTime },
        new DailySchedule { Day = WeekDay.Saturday,  IsClosed = true },
        new DailySchedule { Day = WeekDay.Sunday,    IsClosed = true },
    ];
}
