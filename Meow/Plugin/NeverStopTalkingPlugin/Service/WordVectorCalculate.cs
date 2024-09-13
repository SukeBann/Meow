using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Meow.Plugin.NeverStopTalkingPlugin.Models;
using Vector = MathNet.Numerics.LinearAlgebra.Complex32.Vector;

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
    /// <param name="threshold"></param>
    /// <returns></returns>
    public List<(double similarity, int msgId)> GetSimilarString(List<BagOfWordVector> totalVector,
        BagOfWordVector target, double threshold)
    {

        var vectors = UnfoldVector(target);
        var denseMatrix = UnfoldVectorList(totalVector);

        var newMessageVector = Vector<double>.Build.Dense(vectors);

        // 计算余弦相似度
        var denseMatrixRowCount = denseMatrix.Length;
        var similarities = new List<(double similarity, int msgId)>();
        Parallel.For(0, denseMatrixRowCount, i =>
        {
            var vector = denseMatrix[i].vector;
            var msgId = denseMatrix[i].msgId;
            var cosineSimilarity = CosineSimilarity(newMessageVector, vector);
            if (cosineSimilarity >= threshold && msgId != target.MsgId)
            { 
                similarities.Add((cosineSimilarity, msgId));
            }
        });

        return similarities;
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
        var cosineSimilarity = dotProduct / (magnitudeA * magnitudeB);
        return cosineSimilarity;
    }

    /// <summary>
    /// 根据词袋向量集合获取词袋矩阵
    /// </summary>
    /// <param name="bagOfWordVectors">词袋向量集合</param>
    /// <returns></returns>
    private (Vector<double> vector, int msgId)[] UnfoldVectorList(List<BagOfWordVector> bagOfWordVectors)
    {
        return bagOfWordVectors.Select(x =>
        {
            var vector = Vector<double>.Build.Dense(UnfoldVector(x));
            return (vector, x.MsgId);
        }).ToArray();
    }
}