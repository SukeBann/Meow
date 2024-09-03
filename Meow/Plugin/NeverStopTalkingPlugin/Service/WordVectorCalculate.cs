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
    /// <param name="totalVector">词袋向量集合</param>
    /// <param name="target">计算结果</param>
    /// <returns></returns>
    public List<(double similarity, int msgId)> GetSimilarString(
        List<BagOfWordVector> totalVector, BagOfWordVector target)
    {

        var vectors = UnfoldVector(target);
        var denseMatrix = GetDenseMatrix(totalVector, vectors.Length);

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
            var msgId = totalVector[Array.IndexOf(similarities, similarity)].MsgId;
            result.Add((similarity, msgId));
        }
        return result;
    }

    /// <summary>
    /// 将词袋计算结果展开为向量
    /// </summary>
    /// <param name="target">词袋计算结果</param>
    /// <returns></returns>
    private static double[] UnfoldVector(BagOfWordVector target)
    {
        var doubles = new double[target.MaxCount];
        foreach (var (index, count) in target.VectorElementIndex)
        {
            doubles[index] = count;
        }

        return doubles;
    }

    /// <summary>
    /// 对两个长度向量做相似度余弦计算
    /// </summary>
    /// <param name="vecA"></param>
    /// <param name="vecB"></param>
    /// <returns></returns>
    private static double CosineSimilarity(Vector<double> vecA, Vector<double> vecB)
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

        var rowList = bagOfWordVectors.Select(UnfoldVector).ToList();
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