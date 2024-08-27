using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Meow.Plugin.NeverStopTalkingPlugin.Models;

namespace Meow.Plugin.NeverStopTalkingPlugin.Service;

/// <summary>
/// 词向量计算 
/// </summary>
public class WordVectorCalculate
{
    /// <summary>
    /// 用词袋向量集合生成矩阵, 拿vectors在其中做余弦相似度计算
    /// </summary>
    /// <param name="bagOfWordVectors">词袋向量集合</param>
    /// <param name="vectors">要匹配的向量</param>
    /// <returns></returns>
    public List<(double similarity, int msgId)> GetSimilarString(
        List<BagOfWordVector> bagOfWordVectors, double[] vectors)
    {
        var denseMatrix = GetDenseMatrix(bagOfWordVectors, vectors.Length);

        var newMessageVector = Vector<double>.Build.Dense(vectors);

        // 计算余弦相似度
        var denseMatrixRowCount = denseMatrix.RowCount;
        var similarities = new double[denseMatrixRowCount];
        Parallel.For(0, denseMatrixRowCount, i =>
        {
            var vector = denseMatrix.Row(i);
            similarities[i] = CosineSimilarity(newMessageVector, vector);
        });

        var result = new List<(double similarity, int msgId)>();
        foreach (var similarity in similarities.OrderDescending())
        {
            var msgId = bagOfWordVectors[Array.IndexOf(similarities, similarity)].MsgId;
            result.Add((similarity, msgId));
        }
        return result;
    }

    static double CosineSimilarity(Vector<double> vecA, Vector<double> vecB)
    {
        var dotProduct = vecA.DotProduct(vecB);
        var magnitudeA = vecA.L2Norm();
        var magnitudeB = vecB.L2Norm();
        return dotProduct / (magnitudeA * magnitudeB);
    }

    /// <summary>
    /// 根据词袋向量集合获取词袋矩阵
    /// </summary>
    /// <param name="bagOfWordVectors">词袋向量集合</param>
    /// <param name="columnCount">矩阵列数</param>
    /// <returns></returns>
    private DenseMatrix GetDenseMatrix(List<BagOfWordVector> bagOfWordVectors, int columnCount)
    {
        var rowCount = bagOfWordVectors.Count;
        var denseMatrixArray = new double[rowCount, columnCount];

        var rowList = bagOfWordVectors.Select(x => x.Vector).ToList();
        for (var rowIndex = 0; rowIndex < rowList.Count; rowIndex++)
        {
            for (var columnIndex = 0; columnIndex < rowList[rowIndex].Length; columnIndex++)
            {
                var row = rowList[rowIndex];
                denseMatrixArray[rowIndex, columnIndex] = row[columnIndex];
            }
        }

        return DenseMatrix.OfArray(denseMatrixArray);
    }
}