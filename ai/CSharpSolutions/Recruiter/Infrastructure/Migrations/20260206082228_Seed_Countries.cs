using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Recruiter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Seed_Countries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Country",
                columns: new[] { "Id", "CountryCode", "CreatedAt", "CreatedBy", "DataCenterRegion", "IsActive", "IsEuMember", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { new Guid("00d05a91-b3ba-621f-0162-22668f6674b1"), "IS", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "Iceland", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("036cafaa-8a0f-0a2b-9f70-4fa61ab00030"), "RW", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Rwanda", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("0386b2d4-ca7e-ce5a-c261-b39bb19c6c76"), "TR", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Turkey", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("03f2c6eb-54d6-fbdf-9334-78f314472ebc"), "MX", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Mexico", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("06154e81-2224-2c49-157c-32e8aa0818f0"), "ME", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "Montenegro", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("0a870eec-cf2f-886a-d90f-a6b526fd7c3a"), "DJ", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Djibouti", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("0c4c898c-bba8-39e6-2275-6959fe57cd41"), "RO", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Romania", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("0ceab9d7-5ea6-6453-f297-4a87423a2ece"), "MH", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Oceania", true, false, "Marshall Islands", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("0e714ad9-a068-6cba-75c1-f4669a779f00"), "KH", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Cambodia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("100e890b-aa63-3e27-afa8-b08e97d8ca02"), "DZ", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Algeria", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("12147fcd-da4c-7d52-20ff-3f24d0a36148"), "ER", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Eritrea", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("1303b592-83f3-847e-8d9b-e4f8190b4c6f"), "UY", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Uruguay", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("1581d866-72a4-dadd-3893-1fd8ce0a966f"), "MZ", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Mozambique", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("1757e0ae-bb63-02c2-5cdb-9a11a2dabffd"), "MK", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "North Macedonia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("18abe062-cb88-bd6b-1cdd-cf530f8940d1"), "CR", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Costa Rica", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("196133d2-f9ac-f193-77ef-9ff708fca5de"), "VN", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Viet Nam", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("1aa6cb3f-090d-7b9b-9d35-76ab34e8f01c"), "BD", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Bangladesh", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("1ac91424-27e6-d5a4-33f2-a075e5ee6ee9"), "VE", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Venezuela", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("1b26f65c-fa7d-2157-b154-06332b1b7b5c"), "CM", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Cameroon", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("1b540b1e-1dde-ce04-1754-d11ad482ab5b"), "EC", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Ecuador", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("1c9ce728-cd77-4c52-589f-92ffce951618"), "IL", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Israel", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("1cf7f05d-02c3-3826-41fa-906f4c973e6b"), "UA", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "Ukraine", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("1e2199be-8b9b-5f4b-60c8-3b7b0bbc87bd"), "TT", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Trinidad and Tobago", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("1f23b8b4-e7bb-6737-f1db-8e4ef7b21cd1"), "NE", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Niger", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("1f4895d9-7377-958d-d0c4-2fb046d9f4e8"), "KP", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "North Korea", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("1f9fb03f-eb0a-dc1f-a219-7bf4dad693a6"), "FR", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "France", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("203a8a78-8ced-2c0f-7f68-6a2b04cdc83b"), "KZ", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Kazakhstan", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("22582b2f-f39a-3b75-3f5b-4ee42e3f8298"), "TN", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Tunisia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("25afc399-91a8-4a4b-d277-e8306985a6c8"), "NI", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Nicaragua", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("25b04d6a-525e-8f15-bf86-2b7756a871df"), "MY", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Malaysia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("262ade14-59a2-04bd-1499-f5e966c3eb2b"), "CF", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Central African Republic", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("27be690e-6fb3-e264-1fde-464493c45174"), "AL", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "Albania", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("289bbc7a-9e54-ebc5-19fd-eb0773fc7894"), "WS", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Oceania", true, false, "Samoa", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("2924b906-7e17-00ee-c1dc-7ad672010041"), "AG", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Antigua and Barbuda", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("297a4359-982f-557b-bfa5-23a0f2865fa4"), "FJ", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Oceania", true, false, "Fiji", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("29f2d838-6aad-49fd-47c6-4955661e36e3"), "MN", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Mongolia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("2a7fd7f7-3765-5fda-ad8a-607d6e651b56"), "ZW", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Zimbabwe", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("2d3de0a1-6f5e-ee87-5819-4555e7bf3c4a"), "BT", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Bhutan", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("2dad61f1-cdca-aa0d-aa95-942aa5dba3bd"), "BY", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "Belarus", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("2f712e34-00af-b0ba-10cd-ac3bc7c1ca17"), "BZ", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Belize", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("2fc6c836-19e0-7ce7-93ba-3e9a5a4826b7"), "EE", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Estonia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("30099244-2b8a-7613-848a-71aa130a141a"), "TH", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Thailand", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("300f6953-75ec-774e-21f0-d94467b0a6b3"), "BG", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Bulgaria", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("3169b72e-887a-ab1d-5465-1418b4ba3db5"), "GQ", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Equatorial Guinea", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("32dd4ce8-1e2b-1859-cefb-53799ff0beb7"), "LS", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Lesotho", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("3306913e-855c-b4f9-cca8-714ac69de16b"), "LV", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Latvia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("336cb90d-d360-dd53-240e-be54302a68e4"), "SS", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "South Sudan", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("33dbc83d-41ad-49a7-647c-f1a6a1f5e535"), "GR", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Greece", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("390af5be-51cc-8874-e1c7-94d4c143f860"), "SB", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Oceania", true, false, "Solomon Islands", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("3a4fbe06-4cb4-34e5-7d70-3271ba78e17f"), "LY", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Libya", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("3c491328-c41a-1841-7d39-99504e9a204c"), "SE", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Sweden", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("3cafaf16-ee41-ba1d-a299-744b184d4239"), "BI", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Burundi", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("3d162d29-f5f6-3b01-12e5-00f72c3f0358"), "HN", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Honduras", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("40e32552-8e52-14b4-8973-374b59843ad6"), "LK", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Sri Lanka", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("418db3f7-060b-d924-8a44-93df1a0f1990"), "VU", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Oceania", true, false, "Vanuatu", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("438d2e0c-da1e-3898-84f7-fc833248700c"), "VC", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Saint Vincent and the Grenadines", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("46c7ca91-406d-41ac-8595-8d4a3158023b"), "NZ", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Oceania", true, false, "New Zealand", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("47db5e2f-6e93-2de0-2f47-df3405945a0b"), "KM", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Comoros", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("481aac30-05e4-fdc3-f262-edbf850c371b"), "ZA", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "South Africa", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("48c65066-1a00-96fc-0c26-c3bba7384068"), "IE", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Ireland", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("4925be57-7b77-628c-ecfc-e253f7842750"), "SN", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Senegal", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("4d398a9f-957b-bd13-75f7-8bc5f4373d20"), "TW", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Taiwan", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("4d4ecae6-384d-4ae6-ebaf-f08744c5671e"), "GY", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Guyana", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("4e354b4d-6889-d731-c202-8a5fd2300d6a"), "TD", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Chad", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("4ee361b6-70b3-2b80-8030-a21dea4cb4df"), "KE", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Kenya", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("4f601caa-30e5-f3a8-6ae2-a452a1363b28"), "MC", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "Monaco", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("510a2e2c-8433-9e3e-6915-f825301eef4d"), "MT", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Malta", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("53b9a04d-169b-9c44-28ae-b1a59001f014"), "ST", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Sao Tome and Principe", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("5491b973-b975-d9df-4b21-cfafef639251"), "BF", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Burkina Faso", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("550129b7-b375-97b6-480c-573ca6d1009d"), "IQ", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Iraq", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("57ab52fb-51e4-8d43-894c-3d30e8d1632c"), "CI", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Côte d'Ivoire", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("5904dc5a-757e-e9d2-e7d5-5d701012f8ff"), "GW", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Guinea-Bissau", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("5fb1ef1e-3f8d-a187-7782-15e626ebc32f"), "BO", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Bolivia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("61fd54f3-61d3-d750-5e03-61ab6c818fb2"), "BN", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Brunei Darussalam", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("621707c9-0375-2010-7358-d0c8381c72cb"), "NL", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Netherlands", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("628dc7a5-bf24-a504-d144-c8007b8e8de7"), "HT", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Haiti", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("65d1a108-4b85-9fee-66d6-85b63f79efb1"), "BA", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "Bosnia and Herzegovina", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("66287888-d198-c252-fa0e-4d5774562e8c"), "OM", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Oman", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("669fc131-8847-349a-9e41-9be4f987932f"), "NP", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Nepal", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("66a6845b-0457-80c2-b176-2b5b2aeb8402"), "AZ", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Azerbaijan", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("66ddaf10-b047-7d25-a72f-91828095872e"), "BR", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Brazil", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("6791682c-7815-4380-f547-f3c7aed4a88b"), "MR", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Mauritania", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("68ba8fbb-124a-3dff-c9ad-3b0559d48c1b"), "HK", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Hong Kong", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("69062783-e838-927c-2f79-b4f656f6ab26"), "CH", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "Switzerland", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("6b22b645-5290-75e1-cd22-01934205e9de"), "GB", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "United Kingdom", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("6f3d5967-d29d-bd75-cb63-f269dd3669c2"), "KI", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Oceania", true, false, "Kiribati", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("7092f6fb-8b3c-997e-e0ea-a181ff296fff"), "MV", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Maldives", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("719f5317-1fe1-1bae-b62c-903d4b3b5861"), "PH", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Philippines", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("7432f7c8-f08e-f680-c9e7-a4cc19df00c4"), "PA", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Panama", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("751def4f-43fe-29a6-6aff-8a1c1a0ccf44"), "NA", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Namibia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("76fa318b-947a-3ebb-dcb0-1716d4f3a5b1"), "NG", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Nigeria", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("76ff1c34-8443-96e2-0e06-06e4780af6a2"), "UZ", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Uzbekistan", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("794c81fa-d44f-e38f-9819-4187fbcd8a77"), "QA", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Qatar", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("7a121b7b-b364-caa5-d049-f57baa9ef103"), "TV", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Oceania", true, false, "Tuvalu", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("7de3d21f-1f35-291a-8925-d0c1795813e1"), "KG", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Kyrgyzstan", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("8437629a-c24f-ffe8-67fe-c8f3397eb117"), "SA", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Saudi Arabia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("85aaed84-6994-60d9-a01f-a01059b4c2f3"), "JP", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Japan", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("85bbc7e8-9e68-b11e-387e-be09361b8ffe"), "BW", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Botswana", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("86841191-d6cc-82bf-3952-6fffb9447149"), "LU", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Luxembourg", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("8a14265a-c5ff-85af-a31d-029aec67650d"), "LR", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Liberia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("8a3b4716-5f54-930b-9eb9-1dda1cbcb80c"), "LA", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Lao People's Democratic Republic", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("8a993ddc-f217-8abe-1761-d6a4cab3a1ac"), "SD", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Sudan", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("8aa9ccc4-fd94-96de-9fc0-f38dfe0334d1"), "KW", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Kuwait", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("9046da98-959f-77a2-5611-3226f0d0c8d4"), "HR", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Croatia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("90a56c09-f959-1e1d-c0f9-43a98e1ee9af"), "ZM", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Zambia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("913ff02e-cba2-f3a4-2ac9-513738302f83"), "JO", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Jordan", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("92c9dbbc-34fd-893b-5b09-415a8d2b7925"), "PE", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Peru", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("937c676d-027d-1fe6-9c8e-c05eb7c5f2d9"), "PT", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Portugal", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("940e18cd-2b8b-0b24-d98a-c2d31bebce98"), "UG", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Uganda", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("942e7d8b-9390-c4f7-a69e-2b506c334f05"), "NO", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "Norway", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("952ee40d-6fc2-7a22-3c4c-555c001ed304"), "SO", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Somalia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("9590f298-62e3-449b-9a5b-ad89a754183d"), "DM", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Dominica", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("97c631b7-8114-f02e-afc3-79b64c719f5c"), "TZ", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Tanzania", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("994db6ec-7cb5-98ec-d7db-f3a50e293351"), "KN", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Saint Kitts and Nevis", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("9a8d30ba-f6be-b1cc-81d8-ba31c57afd67"), "PS", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Palestine", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("9bfca75e-bdce-2c14-73a3-37234ae63c02"), "CD", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Democratic Republic of the Congo", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("9cac29d0-2c79-6480-5234-df931e7bd8cb"), "CN", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "China", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("9d3857f6-b3fd-8980-b93c-7bb04c9a8a4c"), "AM", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Armenia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("9d7bd0d2-cde6-ebae-402b-b3b2c89cc74a"), "CA", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Canada", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("9ec418cb-66fa-fee8-c88a-024741cdcb0f"), "IR", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Iran", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("9fb663e6-c35f-aa3e-d631-35182b0fe94c"), "ES", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Spain", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("9fefb9e7-0266-df5f-a023-5b0204a97186"), "IT", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Italy", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("a0ac7942-5148-81c3-8dbd-d5f595871418"), "PG", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Oceania", true, false, "Papua New Guinea", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("a0ca9e7f-ac3c-3d43-9de2-05096cadeb09"), "AT", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Austria", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("a7b15568-84f3-c2d3-65b8-c8ede8a8b363"), "SR", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Suriname", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("a7f8124b-af45-f30b-ba5c-6f2e7f8af977"), "GA", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Gabon", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("a8ab3af3-a0b6-4def-b781-88a205ce7af4"), "DK", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Denmark", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("a913bc32-972e-186a-a670-71b5323957ff"), "RU", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "Russian Federation", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("a93a28b8-77cb-dc40-d463-8886bfb84452"), "SI", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Slovenia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("a9718c86-f66d-cb38-ae61-d10cb9931abb"), "MW", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Malawi", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("a9ebc931-a56b-9583-5867-d64b32bbd134"), "CU", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Cuba", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("ac19e7ec-373b-b223-5989-e3dde3e7bd83"), "CL", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Chile", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("acdefae4-7538-1aec-586e-5d259f9a6d40"), "PW", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Oceania", true, false, "Palau", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("ae329a5d-2856-82a2-c940-bd254c030624"), "BS", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Bahamas", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("b2ac942e-35d3-84c1-6b6c-e9d6add7150d"), "SY", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Syrian Arab Republic", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("b300a94a-b8a1-4934-6aa3-643c68b4f1ee"), "MD", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "Moldova", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("b37b854e-014e-b793-5888-47be5ec558ab"), "GT", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Guatemala", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("b45b3841-a899-be5d-39a5-b365b7801d56"), "CG", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Congo", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("b4d53ded-5a07-ddb8-b484-92a5ac5a6547"), "FM", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Oceania", true, false, "Micronesia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("b4f7155a-42c1-0560-45f8-e0faf72dada9"), "MA", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Morocco", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("b63b5371-ef42-146c-a917-2973e345f32a"), "PY", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Paraguay", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("b72afc2a-7c5d-300a-0881-628af5f697a9"), "SC", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Seychelles", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("b7a61914-01f7-05e6-29aa-d36483456faf"), "PK", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Pakistan", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("b9a94dfc-63f5-6344-8ed9-29f1fea1d898"), "TG", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Togo", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("ba3c73ac-a446-184b-a339-10b47bf4a8ca"), "LI", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "Liechtenstein", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("bc4151be-86e2-c0dc-494a-93fc1b99d283"), "TO", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Oceania", true, false, "Tonga", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("bf05dbc6-b53e-d2a0-b7d5-be0ebbd28806"), "BB", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Barbados", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("c0062c48-b3f0-6e48-8ca7-f1eef72bcc97"), "AE", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "United Arab Emirates", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("c0c22a33-608b-7372-96f0-89f365f0ffc5"), "BH", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Bahrain", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("c3c7b04c-6397-1f7f-c04a-d139fa7310e6"), "GN", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Guinea", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("c57295c1-321d-0775-4704-18848d12db6e"), "LC", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Saint Lucia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("c7ec474c-56a4-71b3-c543-66fcfa30b692"), "TM", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Turkmenistan", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("c9e91d5f-a88d-d45f-f0f6-63e4dce4bc79"), "GE", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Georgia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("cbc0d3e3-2613-6d79-f518-d49c3c260619"), "TJ", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Tajikistan", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("cbfc9807-8010-b07d-030c-4bc5a50e8734"), "DE", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Germany", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("cc24633a-2a3a-d95e-3da2-a8c9324a9c03"), "GM", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Gambia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("cc9e0258-23c6-2db3-da41-28a48e76caf8"), "GH", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Ghana", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("ccc7931e-0631-8112-8155-85a13c3bc7a8"), "PL", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Poland", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("cd5673c1-2f3c-a3f9-149d-3d83e26d72cc"), "PR", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Puerto Rico", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("cd657d4c-c258-c21e-c50b-8a5db0acdeee"), "AD", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "Andorra", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("cf085149-8fe5-1559-e0cc-eca8636701a7"), "CZ", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Czechia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("d5002a8a-74bb-72ae-0282-f4dd56a076ca"), "SG", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Singapore", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("d7f54377-bfe5-7685-37c7-0efdfdb11d29"), "SZ", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Eswatini", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("d84d56b2-dff4-0df9-9386-a1c93b0a29c7"), "ID", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Indonesia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("d87e6d18-6aee-2e28-34dc-1f211f6ee4f2"), "CV", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Cabo Verde", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("d8ab90db-6fde-f7fc-386d-1645e249450b"), "CO", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Colombia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("d9468103-3543-590a-4de3-4f3444d60ea5"), "ET", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Ethiopia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("da109711-bb9b-e5ee-ddfb-cdb7a90a3a14"), "SK", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Slovakia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("da5030df-c6a2-9932-9c78-64d305e17d77"), "US", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "United States of America", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("da84fca4-f7c4-c672-3d81-e2a37b56a84a"), "LT", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Lithuania", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("df90e22f-bfa4-b358-b9f2-d277c859adc7"), "EG", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Egypt", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("dfa07d18-9431-f3e5-a18d-58ce314254fa"), "CY", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, true, "Cyprus", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("e2171ffb-bd6b-28ee-266e-d7daf2432fad"), "GD", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Grenada", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("e2dd46c6-7598-f1f9-6224-5273d0372e86"), "MM", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Myanmar", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("e357ac8e-82fa-0827-0d72-d7220bfd6207"), "MO", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Macao", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("e4778807-e50b-40d4-ce31-bb4e197f1581"), "AR", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Argentina", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("e491e51d-f977-fa93-8b27-ee449b1d5ee6"), "ML", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Mali", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("e6631df6-c576-9af3-50c7-e3d6317c8cf4"), "BJ", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Benin", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("e67b8464-a297-047a-58e9-b1edfbb9aa75"), "LB", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Lebanon", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("e6aa1afd-1bd8-5d70-7b0c-c919fa473425"), "HU", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Hungary", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("e7493dcb-358a-ec04-351f-fec99471ad33"), "AF", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Afghanistan", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("e9ec09e9-3471-88ee-f466-2ee22cfbaf7b"), "FI", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Finland", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("ed9476ee-cd92-e7f2-f2ae-3ff607354d38"), "DO", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Dominican Republic", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("ef6d0b50-170f-5603-9dfe-a662226b2a3e"), "JM", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "Jamaica", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("f0490fbe-6782-3b28-ba97-e20756535d38"), "SV", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Americas", true, false, "El Salvador", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("f1784bb3-1b36-18de-faf5-787519dba5bd"), "BE", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, true, "Belgium", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("f1ccb3d3-5484-1544-5c79-4d3a53560e47"), "AO", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Angola", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("f3e53795-9ef0-ae08-f666-d25a642c8713"), "YE", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Yemen", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("f450ca63-4395-abe9-1675-4b7686d895e6"), "KR", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "South Korea", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("f5a71c0c-c09d-a403-4f67-b03bd69fc763"), "MG", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Madagascar", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("f61b3c96-a1a2-1ded-40ae-247db7b58483"), "MU", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Mauritius", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("f931be5d-9ede-2859-a6dd-8cb8b21bf243"), "IN", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "India", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("f94d94dd-dbb7-f241-dd7c-2e3b7fa4e9b3"), "RS", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "Serbia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("fa1e0414-388e-df0c-0911-b0260a932250"), "AU", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Oceania", true, false, "Australia", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("fbacc12b-dc42-e371-4aac-2155c27f884a"), "TL", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Asia", true, false, "Timor-Leste", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("fbaf93de-eb6b-706f-2833-e0aa52d15357"), "NR", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Oceania", true, false, "Nauru", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("fd805786-50da-be5d-34aa-5cedb7b80044"), "SM", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Europe", true, false, "San Marino", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" },
                    { new Guid("ff13f796-21c6-590b-be49-301ec4d1b351"), "SL", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System", "Africa", true, false, "Sierra Leone", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "System" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("00d05a91-b3ba-621f-0162-22668f6674b1"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("036cafaa-8a0f-0a2b-9f70-4fa61ab00030"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("0386b2d4-ca7e-ce5a-c261-b39bb19c6c76"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("03f2c6eb-54d6-fbdf-9334-78f314472ebc"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("06154e81-2224-2c49-157c-32e8aa0818f0"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("0a870eec-cf2f-886a-d90f-a6b526fd7c3a"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("0c4c898c-bba8-39e6-2275-6959fe57cd41"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("0ceab9d7-5ea6-6453-f297-4a87423a2ece"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("0e714ad9-a068-6cba-75c1-f4669a779f00"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("100e890b-aa63-3e27-afa8-b08e97d8ca02"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("12147fcd-da4c-7d52-20ff-3f24d0a36148"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("1303b592-83f3-847e-8d9b-e4f8190b4c6f"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("1581d866-72a4-dadd-3893-1fd8ce0a966f"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("1757e0ae-bb63-02c2-5cdb-9a11a2dabffd"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("18abe062-cb88-bd6b-1cdd-cf530f8940d1"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("196133d2-f9ac-f193-77ef-9ff708fca5de"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("1aa6cb3f-090d-7b9b-9d35-76ab34e8f01c"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("1ac91424-27e6-d5a4-33f2-a075e5ee6ee9"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("1b26f65c-fa7d-2157-b154-06332b1b7b5c"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("1b540b1e-1dde-ce04-1754-d11ad482ab5b"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("1c9ce728-cd77-4c52-589f-92ffce951618"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("1cf7f05d-02c3-3826-41fa-906f4c973e6b"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("1e2199be-8b9b-5f4b-60c8-3b7b0bbc87bd"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("1f23b8b4-e7bb-6737-f1db-8e4ef7b21cd1"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("1f4895d9-7377-958d-d0c4-2fb046d9f4e8"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("1f9fb03f-eb0a-dc1f-a219-7bf4dad693a6"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("203a8a78-8ced-2c0f-7f68-6a2b04cdc83b"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("22582b2f-f39a-3b75-3f5b-4ee42e3f8298"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("25afc399-91a8-4a4b-d277-e8306985a6c8"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("25b04d6a-525e-8f15-bf86-2b7756a871df"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("262ade14-59a2-04bd-1499-f5e966c3eb2b"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("27be690e-6fb3-e264-1fde-464493c45174"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("289bbc7a-9e54-ebc5-19fd-eb0773fc7894"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("2924b906-7e17-00ee-c1dc-7ad672010041"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("297a4359-982f-557b-bfa5-23a0f2865fa4"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("29f2d838-6aad-49fd-47c6-4955661e36e3"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("2a7fd7f7-3765-5fda-ad8a-607d6e651b56"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("2d3de0a1-6f5e-ee87-5819-4555e7bf3c4a"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("2dad61f1-cdca-aa0d-aa95-942aa5dba3bd"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("2f712e34-00af-b0ba-10cd-ac3bc7c1ca17"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("2fc6c836-19e0-7ce7-93ba-3e9a5a4826b7"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("30099244-2b8a-7613-848a-71aa130a141a"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("300f6953-75ec-774e-21f0-d94467b0a6b3"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("3169b72e-887a-ab1d-5465-1418b4ba3db5"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("32dd4ce8-1e2b-1859-cefb-53799ff0beb7"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("3306913e-855c-b4f9-cca8-714ac69de16b"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("336cb90d-d360-dd53-240e-be54302a68e4"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("33dbc83d-41ad-49a7-647c-f1a6a1f5e535"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("390af5be-51cc-8874-e1c7-94d4c143f860"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("3a4fbe06-4cb4-34e5-7d70-3271ba78e17f"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("3c491328-c41a-1841-7d39-99504e9a204c"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("3cafaf16-ee41-ba1d-a299-744b184d4239"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("3d162d29-f5f6-3b01-12e5-00f72c3f0358"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("40e32552-8e52-14b4-8973-374b59843ad6"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("418db3f7-060b-d924-8a44-93df1a0f1990"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("438d2e0c-da1e-3898-84f7-fc833248700c"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("46c7ca91-406d-41ac-8595-8d4a3158023b"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("47db5e2f-6e93-2de0-2f47-df3405945a0b"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("481aac30-05e4-fdc3-f262-edbf850c371b"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("48c65066-1a00-96fc-0c26-c3bba7384068"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("4925be57-7b77-628c-ecfc-e253f7842750"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("4d398a9f-957b-bd13-75f7-8bc5f4373d20"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("4d4ecae6-384d-4ae6-ebaf-f08744c5671e"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("4e354b4d-6889-d731-c202-8a5fd2300d6a"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("4ee361b6-70b3-2b80-8030-a21dea4cb4df"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("4f601caa-30e5-f3a8-6ae2-a452a1363b28"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("510a2e2c-8433-9e3e-6915-f825301eef4d"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("53b9a04d-169b-9c44-28ae-b1a59001f014"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("5491b973-b975-d9df-4b21-cfafef639251"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("550129b7-b375-97b6-480c-573ca6d1009d"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("57ab52fb-51e4-8d43-894c-3d30e8d1632c"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("5904dc5a-757e-e9d2-e7d5-5d701012f8ff"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("5fb1ef1e-3f8d-a187-7782-15e626ebc32f"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("61fd54f3-61d3-d750-5e03-61ab6c818fb2"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("621707c9-0375-2010-7358-d0c8381c72cb"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("628dc7a5-bf24-a504-d144-c8007b8e8de7"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("65d1a108-4b85-9fee-66d6-85b63f79efb1"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("66287888-d198-c252-fa0e-4d5774562e8c"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("669fc131-8847-349a-9e41-9be4f987932f"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("66a6845b-0457-80c2-b176-2b5b2aeb8402"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("66ddaf10-b047-7d25-a72f-91828095872e"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("6791682c-7815-4380-f547-f3c7aed4a88b"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("68ba8fbb-124a-3dff-c9ad-3b0559d48c1b"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("69062783-e838-927c-2f79-b4f656f6ab26"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("6b22b645-5290-75e1-cd22-01934205e9de"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("6f3d5967-d29d-bd75-cb63-f269dd3669c2"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("7092f6fb-8b3c-997e-e0ea-a181ff296fff"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("719f5317-1fe1-1bae-b62c-903d4b3b5861"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("7432f7c8-f08e-f680-c9e7-a4cc19df00c4"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("751def4f-43fe-29a6-6aff-8a1c1a0ccf44"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("76fa318b-947a-3ebb-dcb0-1716d4f3a5b1"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("76ff1c34-8443-96e2-0e06-06e4780af6a2"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("794c81fa-d44f-e38f-9819-4187fbcd8a77"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("7a121b7b-b364-caa5-d049-f57baa9ef103"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("7de3d21f-1f35-291a-8925-d0c1795813e1"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("8437629a-c24f-ffe8-67fe-c8f3397eb117"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("85aaed84-6994-60d9-a01f-a01059b4c2f3"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("85bbc7e8-9e68-b11e-387e-be09361b8ffe"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("86841191-d6cc-82bf-3952-6fffb9447149"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("8a14265a-c5ff-85af-a31d-029aec67650d"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("8a3b4716-5f54-930b-9eb9-1dda1cbcb80c"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("8a993ddc-f217-8abe-1761-d6a4cab3a1ac"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("8aa9ccc4-fd94-96de-9fc0-f38dfe0334d1"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("9046da98-959f-77a2-5611-3226f0d0c8d4"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("90a56c09-f959-1e1d-c0f9-43a98e1ee9af"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("913ff02e-cba2-f3a4-2ac9-513738302f83"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("92c9dbbc-34fd-893b-5b09-415a8d2b7925"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("937c676d-027d-1fe6-9c8e-c05eb7c5f2d9"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("940e18cd-2b8b-0b24-d98a-c2d31bebce98"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("942e7d8b-9390-c4f7-a69e-2b506c334f05"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("952ee40d-6fc2-7a22-3c4c-555c001ed304"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("9590f298-62e3-449b-9a5b-ad89a754183d"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("97c631b7-8114-f02e-afc3-79b64c719f5c"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("994db6ec-7cb5-98ec-d7db-f3a50e293351"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("9a8d30ba-f6be-b1cc-81d8-ba31c57afd67"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("9bfca75e-bdce-2c14-73a3-37234ae63c02"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("9cac29d0-2c79-6480-5234-df931e7bd8cb"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("9d3857f6-b3fd-8980-b93c-7bb04c9a8a4c"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("9d7bd0d2-cde6-ebae-402b-b3b2c89cc74a"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("9ec418cb-66fa-fee8-c88a-024741cdcb0f"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("9fb663e6-c35f-aa3e-d631-35182b0fe94c"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("9fefb9e7-0266-df5f-a023-5b0204a97186"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("a0ac7942-5148-81c3-8dbd-d5f595871418"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("a0ca9e7f-ac3c-3d43-9de2-05096cadeb09"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("a7b15568-84f3-c2d3-65b8-c8ede8a8b363"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("a7f8124b-af45-f30b-ba5c-6f2e7f8af977"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("a8ab3af3-a0b6-4def-b781-88a205ce7af4"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("a913bc32-972e-186a-a670-71b5323957ff"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("a93a28b8-77cb-dc40-d463-8886bfb84452"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("a9718c86-f66d-cb38-ae61-d10cb9931abb"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("a9ebc931-a56b-9583-5867-d64b32bbd134"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("ac19e7ec-373b-b223-5989-e3dde3e7bd83"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("acdefae4-7538-1aec-586e-5d259f9a6d40"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("ae329a5d-2856-82a2-c940-bd254c030624"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("b2ac942e-35d3-84c1-6b6c-e9d6add7150d"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("b300a94a-b8a1-4934-6aa3-643c68b4f1ee"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("b37b854e-014e-b793-5888-47be5ec558ab"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("b45b3841-a899-be5d-39a5-b365b7801d56"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("b4d53ded-5a07-ddb8-b484-92a5ac5a6547"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("b4f7155a-42c1-0560-45f8-e0faf72dada9"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("b63b5371-ef42-146c-a917-2973e345f32a"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("b72afc2a-7c5d-300a-0881-628af5f697a9"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("b7a61914-01f7-05e6-29aa-d36483456faf"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("b9a94dfc-63f5-6344-8ed9-29f1fea1d898"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("ba3c73ac-a446-184b-a339-10b47bf4a8ca"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("bc4151be-86e2-c0dc-494a-93fc1b99d283"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("bf05dbc6-b53e-d2a0-b7d5-be0ebbd28806"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("c0062c48-b3f0-6e48-8ca7-f1eef72bcc97"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("c0c22a33-608b-7372-96f0-89f365f0ffc5"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("c3c7b04c-6397-1f7f-c04a-d139fa7310e6"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("c57295c1-321d-0775-4704-18848d12db6e"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("c7ec474c-56a4-71b3-c543-66fcfa30b692"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("c9e91d5f-a88d-d45f-f0f6-63e4dce4bc79"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("cbc0d3e3-2613-6d79-f518-d49c3c260619"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("cbfc9807-8010-b07d-030c-4bc5a50e8734"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("cc24633a-2a3a-d95e-3da2-a8c9324a9c03"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("cc9e0258-23c6-2db3-da41-28a48e76caf8"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("ccc7931e-0631-8112-8155-85a13c3bc7a8"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("cd5673c1-2f3c-a3f9-149d-3d83e26d72cc"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("cd657d4c-c258-c21e-c50b-8a5db0acdeee"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("cf085149-8fe5-1559-e0cc-eca8636701a7"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("d5002a8a-74bb-72ae-0282-f4dd56a076ca"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("d7f54377-bfe5-7685-37c7-0efdfdb11d29"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("d84d56b2-dff4-0df9-9386-a1c93b0a29c7"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("d87e6d18-6aee-2e28-34dc-1f211f6ee4f2"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("d8ab90db-6fde-f7fc-386d-1645e249450b"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("d9468103-3543-590a-4de3-4f3444d60ea5"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("da109711-bb9b-e5ee-ddfb-cdb7a90a3a14"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("da5030df-c6a2-9932-9c78-64d305e17d77"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("da84fca4-f7c4-c672-3d81-e2a37b56a84a"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("df90e22f-bfa4-b358-b9f2-d277c859adc7"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("dfa07d18-9431-f3e5-a18d-58ce314254fa"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("e2171ffb-bd6b-28ee-266e-d7daf2432fad"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("e2dd46c6-7598-f1f9-6224-5273d0372e86"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("e357ac8e-82fa-0827-0d72-d7220bfd6207"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("e4778807-e50b-40d4-ce31-bb4e197f1581"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("e491e51d-f977-fa93-8b27-ee449b1d5ee6"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("e6631df6-c576-9af3-50c7-e3d6317c8cf4"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("e67b8464-a297-047a-58e9-b1edfbb9aa75"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("e6aa1afd-1bd8-5d70-7b0c-c919fa473425"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("e7493dcb-358a-ec04-351f-fec99471ad33"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("e9ec09e9-3471-88ee-f466-2ee22cfbaf7b"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("ed9476ee-cd92-e7f2-f2ae-3ff607354d38"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("ef6d0b50-170f-5603-9dfe-a662226b2a3e"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("f0490fbe-6782-3b28-ba97-e20756535d38"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("f1784bb3-1b36-18de-faf5-787519dba5bd"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("f1ccb3d3-5484-1544-5c79-4d3a53560e47"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("f3e53795-9ef0-ae08-f666-d25a642c8713"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("f450ca63-4395-abe9-1675-4b7686d895e6"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("f5a71c0c-c09d-a403-4f67-b03bd69fc763"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("f61b3c96-a1a2-1ded-40ae-247db7b58483"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("f931be5d-9ede-2859-a6dd-8cb8b21bf243"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("f94d94dd-dbb7-f241-dd7c-2e3b7fa4e9b3"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("fa1e0414-388e-df0c-0911-b0260a932250"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("fbacc12b-dc42-e371-4aac-2155c27f884a"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("fbaf93de-eb6b-706f-2833-e0aa52d15357"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("fd805786-50da-be5d-34aa-5cedb7b80044"));

            migrationBuilder.DeleteData(
                table: "Country",
                keyColumn: "Id",
                keyValue: new Guid("ff13f796-21c6-590b-be49-301ec4d1b351"));
        }
    }
}
