using Ddd.Api.OpenApi;
using Ddd.Api.Products.Schemas;
using Ddd.Application.Extensions;
using Ddd.Infrastructure.Extensions;

namespace Ddd.Api.Extensions;

/// <summary>
/// プレゼンテーション層(合成ルート)の依存関係を DI コンテナへ登録する拡張メソッドを提供する。
/// </summary>
/// <remarks>
/// API プロジェクトは合成ルートであり、プレゼンテーション層自身の登録(コントローラ・OpenAPI・スキーマ
/// アダプタ)に加えて、下位層(アプリケーション層・インフラストラクチャ層)の登録も束ねる。
/// これにより <c>Program.cs</c> はサービス登録を <see cref="AddPresentation"/> の1呼び出しに集約できる。
/// </remarks>
public static class DependencyInjection
{
    /// <summary>
    /// プレゼンテーション層および下位層のサービスをまとめて登録する。
    /// </summary>
    /// <param name="services">サービスコレクション。</param>
    /// <param name="configuration">接続文字列等を含む構成。</param>
    /// <returns>連鎖呼び出し用の <paramref name="services"/>。</returns>
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        // --- プレゼンテーション層 ---
        services.AddControllers();
        services.AddApiDocumentation();
        // 商品登録スキーマ → ProductDto のアダプタ(状態を持たないため Singleton)。
        services.AddSingleton<ProductCreateSchemaAdapter>();

        // --- 下位層(合成ルートとして束ねる) ---
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("接続文字列 'Postgres' が設定されていません。");
        services.AddApplication();
        services.AddInfrastructure(connectionString);

        return services;
    }
}