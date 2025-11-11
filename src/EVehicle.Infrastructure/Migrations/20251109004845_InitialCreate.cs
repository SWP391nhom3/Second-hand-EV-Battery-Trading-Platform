using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EVehicle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "Package_Definitions",
                columns: table => new
                {
                    package_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    price = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    credits_count = table.Column<int>(type: "int", nullable: false),
                    priority_level = table.Column<int>(type: "int", nullable: false),
                    max_images = table.Column<int>(type: "int", nullable: false, defaultValue: 5),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Package_Definitions", x => x.package_id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    phone_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    avatar_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    id_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    id_front_image_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    id_back_image_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    social_login_provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    social_login_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "MEMBER"),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "ACTIVE"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "Contract_Templates",
                columns: table => new
                {
                    template_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    template_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    template_content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contract_Templates", x => x.template_id);
                    table.ForeignKey(
                        name: "FK_Contract_Templates_Categories_category_id",
                        column: x => x.category_id,
                        principalTable: "Categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Market_Data",
                columns: table => new
                {
                    data_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    year = table.Column<int>(type: "int", nullable: true),
                    soh_percentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    mileage = table.Column<int>(type: "int", nullable: true),
                    selling_price = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    transaction_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Market_Data", x => x.data_id);
                    table.ForeignKey(
                        name: "FK_Market_Data_Categories_category_id",
                        column: x => x.category_id,
                        principalTable: "Categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    notification_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    notification_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    related_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    is_read = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.notification_id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    price = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    suggested_price = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true),
                    location = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    battery_capacity_current = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    charge_count = table.Column<int>(type: "int", nullable: true),
                    production_year = table.Column<int>(type: "int", nullable: false),
                    condition = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    mileage = table.Column<int>(type: "int", nullable: true),
                    auction_enabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    starting_bid = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true),
                    buy_now_price = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true),
                    auction_end_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PENDING"),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    is_sold = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    rejection_reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    approved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    rejected_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    bumped_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.post_id);
                    table.ForeignKey(
                        name: "FK_Posts_Categories_category_id",
                        column: x => x.category_id,
                        principalTable: "Categories",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Posts_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "User_Package_Credits",
                columns: table => new
                {
                    user_credit_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    package_id = table.Column<int>(type: "int", nullable: false),
                    credits_remaining = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    total_credits = table.Column<int>(type: "int", nullable: false),
                    purchased_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Package_Credits", x => x.user_credit_id);
                    table.ForeignKey(
                        name: "FK_User_Package_Credits_Package_Definitions_package_id",
                        column: x => x.package_id,
                        principalTable: "Package_Definitions",
                        principalColumn: "package_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_User_Package_Credits_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AI_Price_Suggestions",
                columns: table => new
                {
                    suggestion_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    suggested_price = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    confidence_score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    factors = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AI_Price_Suggestions", x => x.suggestion_id);
                    table.ForeignKey(
                        name: "FK_AI_Price_Suggestions_Posts_post_id",
                        column: x => x.post_id,
                        principalTable: "Posts",
                        principalColumn: "post_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bids",
                columns: table => new
                {
                    bid_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    bid_amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    is_winning_bid = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bids", x => x.bid_id);
                    table.ForeignKey(
                        name: "FK_Bids_Posts_post_id",
                        column: x => x.post_id,
                        principalTable: "Posts",
                        principalColumn: "post_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bids_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Favorites",
                columns: table => new
                {
                    favorite_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favorites", x => x.favorite_id);
                    table.ForeignKey(
                        name: "FK_Favorites_Posts_post_id",
                        column: x => x.post_id,
                        principalTable: "Posts",
                        principalColumn: "post_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favorites_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    lead_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    buyer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    staff_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    assigned_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    lead_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "SCHEDULE_VIEW"),
                    status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "NEW"),
                    final_price = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: true),
                    assigned_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    closed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.lead_id);
                    table.ForeignKey(
                        name: "FK_Leads_Posts_post_id",
                        column: x => x.post_id,
                        principalTable: "Posts",
                        principalColumn: "post_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leads_Users_assigned_by",
                        column: x => x.assigned_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leads_Users_buyer_id",
                        column: x => x.buyer_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leads_Users_staff_id",
                        column: x => x.staff_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Post_Images",
                columns: table => new
                {
                    image_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    image_url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    is_thumbnail = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    is_proof = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    display_order = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Post_Images", x => x.image_id);
                    table.ForeignKey(
                        name: "FK_Post_Images_Posts_post_id",
                        column: x => x.post_id,
                        principalTable: "Posts",
                        principalColumn: "post_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Post_Staff_Assignments",
                columns: table => new
                {
                    assignment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    staff_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assigned_by = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Post_Staff_Assignments", x => x.assignment_id);
                    table.ForeignKey(
                        name: "FK_Post_Staff_Assignments_Posts_post_id",
                        column: x => x.post_id,
                        principalTable: "Posts",
                        principalColumn: "post_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Post_Staff_Assignments_Users_assigned_by",
                        column: x => x.assigned_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Post_Staff_Assignments_Users_staff_id",
                        column: x => x.staff_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Product_Comparisons",
                columns: table => new
                {
                    comparison_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    session_id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product_Comparisons", x => x.comparison_id);
                    table.ForeignKey(
                        name: "FK_Product_Comparisons_Posts_post_id",
                        column: x => x.post_id,
                        principalTable: "Posts",
                        principalColumn: "post_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Product_Comparisons_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Post_Subscriptions",
                columns: table => new
                {
                    subscription_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_credit_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    package_id = table.Column<int>(type: "int", nullable: false),
                    credits_used = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    applied_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Post_Subscriptions", x => x.subscription_id);
                    table.ForeignKey(
                        name: "FK_Post_Subscriptions_Package_Definitions_package_id",
                        column: x => x.package_id,
                        principalTable: "Package_Definitions",
                        principalColumn: "package_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Post_Subscriptions_Posts_post_id",
                        column: x => x.post_id,
                        principalTable: "Posts",
                        principalColumn: "post_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Post_Subscriptions_User_Package_Credits_user_credit_id",
                        column: x => x.user_credit_id,
                        principalTable: "User_Package_Credits",
                        principalColumn: "user_credit_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    appointment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    lead_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    buyer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    seller_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    staff_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    start_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "CONFIRMED"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.appointment_id);
                    table.ForeignKey(
                        name: "FK_Appointments_Leads_lead_id",
                        column: x => x.lead_id,
                        principalTable: "Leads",
                        principalColumn: "lead_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Posts_post_id",
                        column: x => x.post_id,
                        principalTable: "Posts",
                        principalColumn: "post_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Users_buyer_id",
                        column: x => x.buyer_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Users_seller_id",
                        column: x => x.seller_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Appointments_Users_staff_id",
                        column: x => x.staff_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Rooms",
                columns: table => new
                {
                    room_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    lead_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    buyer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    seller_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    staff_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    last_message_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Rooms", x => x.room_id);
                    table.ForeignKey(
                        name: "FK_Chat_Rooms_Leads_lead_id",
                        column: x => x.lead_id,
                        principalTable: "Leads",
                        principalColumn: "lead_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Chat_Rooms_Posts_post_id",
                        column: x => x.post_id,
                        principalTable: "Posts",
                        principalColumn: "post_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Chat_Rooms_Users_buyer_id",
                        column: x => x.buyer_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Chat_Rooms_Users_seller_id",
                        column: x => x.seller_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Chat_Rooms_Users_staff_id",
                        column: x => x.staff_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    lead_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    post_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    buyer_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    seller_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    staff_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    final_price = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "PENDING_PAYMENT"),
                    payment_method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    shipping_address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    paid_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.order_id);
                    table.ForeignKey(
                        name: "FK_Orders_Leads_lead_id",
                        column: x => x.lead_id,
                        principalTable: "Leads",
                        principalColumn: "lead_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Posts_post_id",
                        column: x => x.post_id,
                        principalTable: "Posts",
                        principalColumn: "post_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Users_buyer_id",
                        column: x => x.buyer_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Users_seller_id",
                        column: x => x.seller_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Orders_Users_staff_id",
                        column: x => x.staff_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Chat_Messages",
                columns: table => new
                {
                    message_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    room_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    message_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "TEXT"),
                    is_read = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chat_Messages", x => x.message_id);
                    table.ForeignKey(
                        name: "FK_Chat_Messages_Chat_Rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "Chat_Rooms",
                        principalColumn: "room_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chat_Messages_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    contract_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    lead_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    contract_template_id = table.Column<int>(type: "int", nullable: true),
                    created_by = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    contract_content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    buyer_signature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    seller_signature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    buyer_signed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    seller_signed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    contract_pdf_url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "DRAFT"),
                    signed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.contract_id);
                    table.ForeignKey(
                        name: "FK_Contracts_Contract_Templates_contract_template_id",
                        column: x => x.contract_template_id,
                        principalTable: "Contract_Templates",
                        principalColumn: "template_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Leads_lead_id",
                        column: x => x.lead_id,
                        principalTable: "Leads",
                        principalColumn: "lead_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Orders_order_id",
                        column: x => x.order_id,
                        principalTable: "Orders",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Users_created_by",
                        column: x => x.created_by,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    payment_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_credit_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    package_id = table.Column<int>(type: "int", nullable: true),
                    amount = table.Column<decimal>(type: "decimal(15,2)", precision: 15, scale: 2, nullable: false),
                    payment_gateway = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    transaction_code = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    payment_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.payment_id);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_order_id",
                        column: x => x.order_id,
                        principalTable: "Orders",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Package_Definitions_package_id",
                        column: x => x.package_id,
                        principalTable: "Package_Definitions",
                        principalColumn: "package_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_User_Package_Credits_user_credit_id",
                        column: x => x.user_credit_id,
                        principalTable: "User_Package_Credits",
                        principalColumn: "user_credit_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    rating_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    order_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    rater_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ratee_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ratee_role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    score = table.Column<int>(type: "int", nullable: false),
                    comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.rating_id);
                    table.ForeignKey(
                        name: "FK_Ratings_Orders_order_id",
                        column: x => x.order_id,
                        principalTable: "Orders",
                        principalColumn: "order_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ratings_Users_ratee_id",
                        column: x => x.ratee_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ratings_Users_rater_id",
                        column: x => x.rater_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Rating_Replies",
                columns: table => new
                {
                    reply_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    rating_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    reply_content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rating_Replies", x => x.reply_id);
                    table.ForeignKey(
                        name: "FK_Rating_Replies_Ratings_rating_id",
                        column: x => x.rating_id,
                        principalTable: "Ratings",
                        principalColumn: "rating_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Rating_Replies_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AI_Price_Suggestions_post_id",
                table: "AI_Price_Suggestions",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_buyer_id",
                table: "Appointments",
                column: "buyer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_lead_id",
                table: "Appointments",
                column: "lead_id");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_post_id",
                table: "Appointments",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_seller_id",
                table: "Appointments",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_staff_id",
                table: "Appointments",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_start_time",
                table: "Appointments",
                column: "start_time");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_status",
                table: "Appointments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_post_id",
                table: "Bids",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_user_id",
                table: "Bids",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_code",
                table: "Categories",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_name",
                table: "Categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Messages_room_id",
                table: "Chat_Messages",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Messages_user_id",
                table: "Chat_Messages",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Rooms_buyer_id",
                table: "Chat_Rooms",
                column: "buyer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Rooms_lead_id",
                table: "Chat_Rooms",
                column: "lead_id");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Rooms_post_id",
                table: "Chat_Rooms",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Rooms_seller_id",
                table: "Chat_Rooms",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Rooms_staff_id",
                table: "Chat_Rooms",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_Contract_Templates_category_id",
                table: "Contract_Templates",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_contract_template_id",
                table: "Contracts",
                column: "contract_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_created_by",
                table: "Contracts",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_lead_id",
                table: "Contracts",
                column: "lead_id");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_order_id",
                table: "Contracts",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_post_id",
                table: "Favorites",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_Favorites_user_id_post_id",
                table: "Favorites",
                columns: new[] { "user_id", "post_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leads_assigned_by",
                table: "Leads",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_buyer_id",
                table: "Leads",
                column: "buyer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_lead_type",
                table: "Leads",
                column: "lead_type");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_post_id",
                table: "Leads",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_staff_id",
                table: "Leads",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_status",
                table: "Leads",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_Market_Data_category_id",
                table: "Market_Data",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_user_id",
                table: "Notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_buyer_id",
                table: "Orders",
                column: "buyer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_lead_id",
                table: "Orders",
                column: "lead_id");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_post_id",
                table: "Orders",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_seller_id",
                table: "Orders",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_staff_id",
                table: "Orders",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_status",
                table: "Orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_Package_Definitions_name",
                table: "Package_Definitions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_created_at",
                table: "Payments",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_order_id",
                table: "Payments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_package_id",
                table: "Payments",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_payment_type",
                table: "Payments",
                column: "payment_type");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_status",
                table: "Payments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_user_credit_id",
                table: "Payments",
                column: "user_credit_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_user_id",
                table: "Payments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Post_Images_post_id",
                table: "Post_Images",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_Post_Staff_Assignments_assigned_by",
                table: "Post_Staff_Assignments",
                column: "assigned_by");

            migrationBuilder.CreateIndex(
                name: "IX_Post_Staff_Assignments_post_id",
                table: "Post_Staff_Assignments",
                column: "post_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Post_Staff_Assignments_staff_id",
                table: "Post_Staff_Assignments",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_Post_Subscriptions_package_id",
                table: "Post_Subscriptions",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "IX_Post_Subscriptions_post_id",
                table: "Post_Subscriptions",
                column: "post_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Post_Subscriptions_user_credit_id",
                table: "Post_Subscriptions",
                column: "user_credit_id");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_brand",
                table: "Posts",
                column: "brand");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_bumped_at",
                table: "Posts",
                column: "bumped_at");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_category_id",
                table: "Posts",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_category_id_brand_model",
                table: "Posts",
                columns: new[] { "category_id", "brand", "model" });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_created_at",
                table: "Posts",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_is_active",
                table: "Posts",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_is_sold",
                table: "Posts",
                column: "is_sold");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_location",
                table: "Posts",
                column: "location");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_model",
                table: "Posts",
                column: "model");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_price",
                table: "Posts",
                column: "price");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_production_year",
                table: "Posts",
                column: "production_year");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_status",
                table: "Posts",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_status_is_active_is_sold",
                table: "Posts",
                columns: new[] { "status", "is_active", "is_sold" });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_user_id",
                table: "Posts",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Comparisons_post_id",
                table: "Product_Comparisons",
                column: "post_id");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Comparisons_user_id",
                table: "Product_Comparisons",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Rating_Replies_rating_id",
                table: "Rating_Replies",
                column: "rating_id");

            migrationBuilder.CreateIndex(
                name: "IX_Rating_Replies_user_id",
                table: "Rating_Replies",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_order_id_rater_id_ratee_id",
                table: "Ratings",
                columns: new[] { "order_id", "rater_id", "ratee_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ratee_id",
                table: "Ratings",
                column: "ratee_id");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_rater_id",
                table: "Ratings",
                column: "rater_id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Package_Credits_package_id",
                table: "User_Package_Credits",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Package_Credits_user_id",
                table: "User_Package_Credits",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_User_Package_Credits_user_id_package_id",
                table: "User_Package_Credits",
                columns: new[] { "user_id", "package_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                table: "Users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_phone_number",
                table: "Users",
                column: "phone_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_role",
                table: "Users",
                column: "role");

            migrationBuilder.CreateIndex(
                name: "IX_Users_status",
                table: "Users",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AI_Price_Suggestions");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "Bids");

            migrationBuilder.DropTable(
                name: "Chat_Messages");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "Favorites");

            migrationBuilder.DropTable(
                name: "Market_Data");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Post_Images");

            migrationBuilder.DropTable(
                name: "Post_Staff_Assignments");

            migrationBuilder.DropTable(
                name: "Post_Subscriptions");

            migrationBuilder.DropTable(
                name: "Product_Comparisons");

            migrationBuilder.DropTable(
                name: "Rating_Replies");

            migrationBuilder.DropTable(
                name: "Chat_Rooms");

            migrationBuilder.DropTable(
                name: "Contract_Templates");

            migrationBuilder.DropTable(
                name: "User_Package_Credits");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "Package_Definitions");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Leads");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
